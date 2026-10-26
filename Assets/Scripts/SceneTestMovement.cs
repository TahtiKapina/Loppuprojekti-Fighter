using System;
using UnityEngine;

public class SceneTestMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private int maxJumps = 1;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;

    private bool isHitStunned = false;
    private float hitstunTimer = 0.3f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isHitStunned)
        {
            hitstunTimer -= Time.deltaTime;
            if (hitstunTimer <= 0f)
            {
                isHitStunned = false;
                hitstunTimer = 0.3f; // Reset for next hit
            }
            return;
        }

        // Get horizontal input (A/D or Left/Right Arrow)
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Ground Check
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            maxJumps = isGrounded ? 2 : maxJumps; // Reset jumps when grounded
        }

        // Jump Input
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            Console.WriteLine("Jump happened");
        }
        if (Input.GetKeyDown(KeyCode.W) && !isGrounded && maxJumps > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            maxJumps--;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }
    public void ApplyHitstun(float duration)
    {
        isHitStunned = true;
        hitstunTimer = duration;
    }

    private void OnDrawGizmosSelected()
    {
        // Visual indicator in Scene view for Ground Check
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}