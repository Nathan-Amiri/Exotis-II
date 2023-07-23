using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using System;

public class PlayerMovement : NetworkBehaviour
{
    //MOVEMENT WITHOUT ACCELERATION values:
    [NonSerialized] public readonly float moveSpeed = 2.5f; //read by distortion
    private readonly float drag = 5; //only applies to x environmental forces, not x movement

    private readonly float jumpForce = 7.2f;
    private readonly float jumpHeight = 1.2f;
    [NonSerialized] public float speedIncrease = 1; //read by distortion and takeflight
    private readonly float speedMultiplier = 1.3f;
    private float startingY = -10; //-10 = null. Used when speed changes mid-jump

    private readonly float lowJumpMultiplier = 4; //used for dynamic jump
    [NonSerialized] public readonly float fallMultiplier = .7f; //fastfall, read by distortion

    [NonSerialized] public bool isGrounded; //set by GroundCheck, read by VenomAbilities

    [NonSerialized] public bool isStunned; //stunned = cannot act. read by player
    private int stunStrength;
    private bool isFrozen; //freeze = frozen in place
    private int freezeStrength;

    private bool hasJump;

    private float environmentalVelocity; //x velocity applied by outside sources

    private int weightlessStrength;

    public Rigidbody2D rb; //assigned in inspector, read by player, used by swoop
    
    private float moveInput;
    private bool jumpInputDown;
    private bool jumpInput;
    private bool jumpBuffering;

    private void Start()
    {
        UpdateGravityScale();
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (isStunned)
        {
            moveInput = 0;
            jumpInput = false;
            jumpInputDown = false;
            return;
        }

        moveInput = isStunned ? 0 : Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
            jumpInputDown = true;

        jumpInput = Input.GetButton("Jump"); //used for dynamic jump
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
            return;

        if (!isFrozen)
        {
            //x movement
            float moveForce = moveSpeed * speedIncrease * moveInput;
            rb.velocity = new Vector2(moveForce + environmentalVelocity, rb.velocity.y);

            //x environmental drag
            environmentalVelocity *= 1 - drag * Time.fixedDeltaTime;
        }

        if (rb.gravityScale != 0) //turn off fastfall and dymanic jump when gravityless
        {
            if (rb.velocity.y < 0)
                //fastfall not multiplied by speedIncrease to make walljumping easier when speed is high
                rb.velocity += (fallMultiplier - 1) * Physics2D.gravity.y * Time.deltaTime * Vector2.up;
            else if (rb.velocity.y > 0 && !jumpInput)
                rb.velocity += speedIncrease * (lowJumpMultiplier - 1) * Physics2D.gravity.y * Time.deltaTime * Vector2.up;
        }

        if (isGrounded)
            hasJump = true;

        if (jumpInputDown || jumpBuffering)
        {
            if (hasJump)
            {
                startingY = transform.position.y;
                rb.velocity = new(rb.velocity.x, speedIncrease * jumpForce);

                StopCoroutine(JumpBuffer());
                jumpBuffering = false;
                StartCoroutine(RemoveJump());
            }
            else if (jumpInputDown)
                StartCoroutine(JumpBuffer());

            jumpInputDown = false;
        }

        if (startingY != -10 && rb.velocity.y < 0) //reset when jump is no longer going up
            startingY = -10;
    }
    private IEnumerator RemoveJump()
    {
        yield return new WaitForSeconds(.1f);
        hasJump = false;
    }

    private void UpdateGravityScale()
    {
        rb.gravityScale = Mathf.Pow(jumpForce * speedIncrease, 2) / (2 * -Physics2D.gravity.y * jumpHeight); //variation of 'Velocity = sqrt(2 * Jump Height * Gravity)'
    }

    private IEnumerator JumpBuffer()
    {
        jumpBuffering = true;
        yield return new WaitForSeconds(.12f);
        jumpBuffering = false;
    }


    //public methods: (other classes will never change playermovement variables except via these methods (and groundcheck))
    public void SpeedChange(bool multiply, int amount) //amount = number of stages (-2, -1, 1, or 2)
    {
        for (int i = 0; i < amount; i++) //first update speedIncrease
            if (multiply)
                speedIncrease *= speedMultiplier;
            else
                speedIncrease /= speedMultiplier;

        UpdateGravityScale(); //using updated speedIncrease, update gravityScale

        if (startingY != -10) //using updated gravityScale, if player is mid-jump, change their velocity to achieve the jump's intended height in new gravity:
        {
            float newHeight = Mathf.Abs(jumpHeight - Mathf.Abs(startingY - transform.position.y)); //these values shouldn't ever be negative. Mathf.abs is just a precaution
            float newVelocity = Mathf.Sqrt(2 * -Physics2D.gravity.y * rb.gravityScale * newHeight); //variation of 'Velocity = sqrt(2 * Jump Height * Gravity)'
            rb.velocity = new(rb.velocity.x, newVelocity); //because newVelocity isn't multiplied by speedIncrease here, speedIncrease isn't included in the equation above
        }
    }

    public void ToggleStun(bool on) //prevents player from acting
    {
        if (on)
            stunStrength += 1;
        else
            stunStrength -= 1;

        on = stunStrength > 0;

        if (on)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            environmentalVelocity = 0;
        }

        isStunned = on;
    }

    public void ToggleFreeze(bool on) //locks player in place
    {
        if (on)
            freezeStrength += 1;
        else
            freezeStrength -= 1;

        on = freezeStrength > 0;

        if (on)
        {
            environmentalVelocity = 0;
            rb.velocity = Vector2.zero;
        }

        isFrozen = on;
        ToggleGravity(!on);
    }

    public void ToggleGravity(bool on)
    {
        if (!on)
            weightlessStrength += 1;
        else
            weightlessStrength -= 1;

        if (weightlessStrength > 0)
            rb.gravityScale = 0;
        else
            UpdateGravityScale();
    }

    public void GiveJump()
    {
        hasJump = true;
    }

    public void AddNewForce(Vector2 force)
    {
        environmentalVelocity += force.x;
        rb.velocity += new Vector2(0, force.y);
    }
}