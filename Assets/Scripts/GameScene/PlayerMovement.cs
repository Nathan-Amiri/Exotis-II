using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using System;

public class PlayerMovement : NetworkBehaviour
{
    [NonSerialized] public readonly float moveSpeed = 2.5f; //read by distortion
    private float moveForce; //x velocity is divided into moveForce and environmentalForce
    private readonly float drag = 10; //only applies to environmental forces, not movement

    private readonly float jumpForce = 7.2f;
    private readonly float jumpHeight = 1.2f;
    [NonSerialized] public float speedIncrease = 1; //read by distortion and takeflight
    private readonly float speedMultiplier = 1.3f;
    private float startingY = -10; //-10 = null. Used when speed changes mid-jump

    private readonly float lowJumpMultiplier = 4; //used for dynamic jump
    [NonSerialized] public readonly float fallMultiplier = .7f; //fastfall, read by distortion

    private bool hasJump;
    [NonSerialized] public bool isGrounded; //read by GroundCheck and VenomAbilities

    [NonSerialized] public bool isStunned; //read by player

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

        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
            jumpInputDown = true;

        jumpInput = Input.GetButton("Jump"); //used for dynamic jump
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
            return;

        if (!isStunned)
        {
            //first, get environmentalForce using previous moveForce value, BEFORE updating moveForce.
            //If moveForce is opposed by an opposite force, (e.g. the player is moving into a wall) set
            //environmentalForce to 0 rather than to the opposite force
            float environmentalForce;
            if (Mathf.Abs(rb.velocity.x) < moveSpeed * speedIncrease)
                environmentalForce = 0;
            else
                environmentalForce = rb.velocity.x - moveForce;

            //second, decay environmentalForce. This step is only unnecessary if the game has no drag/friction already
            //if there's no room to decay further, drop to zero and stay there
            if (Mathf.Abs(environmentalForce) < drag)
                environmentalForce = 0;
            //otherwise, decay
            else
                environmentalForce = (Mathf.Abs(environmentalForce) - drag * Time.fixedDeltaTime) * Mathf.Sign(environmentalForce);

            //third, update moveForce to match any changes to moveInput
            moveForce = moveInput * moveSpeed * speedIncrease;

            //fourth, update velocity using updated forces
            rb.velocity = new Vector2(environmentalForce + moveForce, rb.velocity.y);
        }

        if (rb.gravityScale != 0) //turn off fastfall and dymanic jump when gravityless
        {
            if (rb.velocity.y < 0)
                rb.velocity += (fallMultiplier - 1) * Physics2D.gravity.y * Time.deltaTime * Vector2.up; //fastfall not multiplied by speedIncrease to make walljumping easier when speed is high
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

    //public IEnumerator CoyoteTime() //called by GroundCheck
    //{
    //    yield return new WaitForSeconds(.07f);
    //    isGrounded = false;
    //}

    public void ToggleStun(bool toggleOn)
    {
        if (toggleOn)
            rb.velocity = new Vector2(0, rb.velocity.y);

        isStunned = toggleOn;
    }

    public void ToggleGravity(bool toggleOn)
    {
        if (!toggleOn)
            weightlessStrength += 1;
        else
            weightlessStrength -= 1;

        if (weightlessStrength > 0)
            rb.gravityScale = 0;
        else
            UpdateGravityScale();
    }
}