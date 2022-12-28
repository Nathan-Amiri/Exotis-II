using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class PlayerMovement : NetworkBehaviour
{
    private float speed;
    private float jumpForce;

    private float fallMultiplier; //fastfall
    private float lowJumpMultiplier; //used for dynamic jump

    [HideInInspector] public bool isGrounded; //read by GroundCheck

    private Rigidbody2D rb;
    
    private float moveInput;
    private bool jumpInputDown;
    private bool jumpInput;
    private bool jumpBuffering;

    private void Awake()
    {
        speed = 3;
        jumpForce = 7;

        fallMultiplier = 1f;
        lowJumpMultiplier = 4f;

        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!IsOwner)
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
}