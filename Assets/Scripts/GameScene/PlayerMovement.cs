using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using System;

public class PlayerMovement : NetworkBehaviour
{
    //MOVEMENT WITHOUT ACCELERATION values:
    [NonSerialized] public readonly float defaultMoveSpeed = 2.5f; //read by distortion
    private float moveForce; //x velocity is divided into moveForce and environmentalForce
    private readonly float drag = 5; //only applies to environmental forces, not movement

    private readonly float jumpForce = 7.2f;
    private readonly float jumpHeight = 1.2f;
    [NonSerialized] public float speedIncrease = 1; //read by distortion and takeflight
    private readonly float speedMultiplier = 1.3f;
    private float startingY = -10; //-10 = null. Used when speed changes mid-jump

    private readonly float lowJumpMultiplier = 4; //used for dynamic jump
    [NonSerialized] public readonly float fallMultiplier = .7f; //fastfall, read by distortion

    private bool hasJump;
    [NonSerialized] public bool isGrounded; //set by GroundCheck, read by VenomAbilities

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
            float moveSpeed = defaultMoveSpeed * speedIncrease;

            //MOVEMENT WITHOUT ACCELERATION: My 4-step method to moving by directly altering the player's
            //velocity while still allowing other forces in the environment to affect the player:
            //1: separate velocity into moveForce and environmentalForce so they can be handled separately
            //2: decay environmentalForce using custom drag
            //3: change moveForce based on moveInput
            //4: re-combine moveForce and environmentalForce to get the new velocity

            //1. Get the sum of all current environmental forces by subtracting the force of your last movement
            //from the total velocity
            float environmentalForce = rb.velocity.x - moveForce;
            //If moveForce is opposed by an equal and opposite force, (e.g. the player is moving into a wall) set
            //environmentalForce to 0 rather than to the opposite force
            if (Mathf.Abs(rb.velocity.x) < 1)
                environmentalForce = 0;

            //2. Decay environmentalForce. This step is unnecessary if the game has drag/friction in it
            //already. However, using rb.drag will cause the player to have more drag if they're moving
            //horizontally in the same direction as any environmental forces, so if you want drag to affect
            //only the environmental forces, better to handle it yourself as shown below
            if (Mathf.Abs(environmentalForce) < .1f) //once a minimum speed is reached, drop to zero
                environmentalForce = 0;
            else
                environmentalForce *= 1 - drag * Time.deltaTime;

            //3. Update moveForce to match any changes to moveInput, caching it for step 1 next FixedUpdate
            moveForce = moveInput * moveSpeed;

            //4. Update velocity using updated forces
            rb.velocity = new Vector2(environmentalForce + moveForce, rb.velocity.y);

            //Movement without acceleration in 4 lines of code:
            //float environmentalForce = Mathf.Abs(rb.velocity.x) < 1 ? 0 : rb.velocity.x - moveForce; //1
            //environmentalForce *= Mathf.Abs(environmentalForce) < .1f ? 0 : 1 - drag * Time.deltaTime; //2
            //moveForce = moveInput * moveSpeed; //3
            //rb.velocity = new Vector2(environmentalForce + moveForce, rb.velocity.y); //4
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

    public void GiveJump() //called by Electrify
    {
        hasJump = true;
    }
}