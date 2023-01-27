using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class PlayerMovement : NetworkBehaviour
{
    [HideInInspector] public float speed = 3; //changed by Player
    [HideInInspector] public float jumpForce = 5; //^

    [HideInInspector] public float lowJumpMultiplier = 4; //^, used for dynamic jump
    private readonly float fallMultiplier = 1; //fastfall

    [HideInInspector] public bool isGrounded; //read by GroundCheck
    [SyncVar]
    [HideInInspector] public bool isStunned; //read by player

    public Rigidbody2D rb; //assigned in inspector
    
    private float moveInput;
    private bool jumpInputDown;
    private bool jumpInput;
    private bool jumpBuffering;

    private void Update()
    {
        if (!IsOwner || !IsClient)
            return;

        if (isStunned)
            return;

        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
            jumpInputDown = true;

        jumpInput = Input.GetButton("Jump"); //used for dynamic jump
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
            return;

        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);

        if (rb.velocity.y < 0)
            rb.velocity += (fallMultiplier - 1) * Physics2D.gravity.y * Time.deltaTime * Vector2.up;
        else if (rb.velocity.y > 0 && !jumpInput)
            rb.velocity += (lowJumpMultiplier - 1) * Physics2D.gravity.y * Time.deltaTime * Vector2.up;

        Jump();
    }

    private void Jump() //run in FixedUpdate
    {
        if (jumpInputDown || jumpBuffering)
        {
            jumpInputDown = false;

            if (isGrounded)
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
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
    public void BecomeStunned(float duration, bool permanent)
    {
        isStunned = true;
        RpcClientBecomeStunned(duration, permanent);
    }

    [ObserversRpc]
    private void RpcClientBecomeStunned(float duration, bool permanent)
    {
        if (IsOwner)
        {
            if (permanent)
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            else
                StartCoroutine(Stun(duration));
        }
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