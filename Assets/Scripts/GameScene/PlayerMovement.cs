using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;

public class PlayerMovement : NetworkBehaviour
{
    [NonSerialized] public float speedIncrease = 1; //changed by Player, all velocity changes are multiplied by speed
    private readonly float moveSpeed = 2.5f;
    private readonly float jumpForce = 6.8f;

    private readonly float lowJumpMultiplier = 4; //used for dynamic jump
    private readonly float fallMultiplier = 1; //fastfall

    [NonSerialized] public bool isGrounded; //read by GroundCheck
    [SyncVar]
    [NonSerialized] public bool isStunned; //read by player

    public Rigidbody2D rb; //assigned in inspector, read by player
    
    private float moveInput;
    private bool jumpInputDown;
    private bool jumpInput;
    private bool jumpBuffering;

    private void Update()
    {
        if (!IsOwner)
            return;
        Debug.Log("Speed: " + moveSpeed);
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

        rb.velocity = new Vector2(moveInput * moveSpeed * speedIncrease, rb.velocity.y);

        if (rb.velocity.y < 0)
            rb.velocity += speedIncrease * (fallMultiplier - 1) * Physics2D.gravity.y * Time.deltaTime * Vector2.up;
        else if (rb.velocity.y > 0 && !jumpInput)
            rb.velocity += speedIncrease * (lowJumpMultiplier - 1) * Physics2D.gravity.y * Time.deltaTime * Vector2.up;

        Jump();
    }

    private void ChangeVelocity(float amount, Vector2 direction)
    {
        rb.velocity = speedIncrease * amount * direction;
    }

    private void Jump() //run in FixedUpdate
    {
        if (jumpInputDown || jumpBuffering)
        {
            jumpInputDown = false;

            if (isGrounded)
                ChangeVelocity(jumpForce, Vector2.up);
            else
            {
                if (jumpBuffering)
                    StopCoroutine(JumpBuffer());
                StartCoroutine(JumpBuffer());
            }
        }
    }

    private IEnumerator JumpBuffer()
    {
        jumpBuffering = true;
        yield return new WaitForSeconds(.12f);
        jumpBuffering = false;
    }

    public IEnumerator CoyoteTime() //called by GroundCheck
    {
        yield return new WaitForSeconds(.07f);
        isGrounded = false;
    }

    [Server]
    public void TemporaryStun(float duration)
    {
        isStunned = true;
        RpcClientBecomeStunned(duration);
    }

    [ObserversRpc]
    private void RpcClientBecomeStunned(float duration)
    {
        if (IsOwner)
            StartCoroutine(Stun(duration));
    }
    public IEnumerator Stun(float duration) //called by player
    {
        Vector2 cachedVelocity = rb.velocity;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        yield return new WaitForSeconds(duration);
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.velocity = cachedVelocity;
        EndStun();
    }
    [ServerRpc]
    private void EndStun()
    {
        isStunned = false;
    }
}