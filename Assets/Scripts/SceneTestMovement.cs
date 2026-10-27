using System;
using UnityEngine;

public class SceneTestMovement : MonoBehaviour
{
    [Header("Player Setup")]
    [SerializeField] private int playerID;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private int maxJumps = 1;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Knockback Braking Feel")]
    [Range(0.5f, 0.98f)]
    [SerializeField] private float brakingMultiplier = 0.85f; // Lower = harder/faster brakes
    [Range(0.1f, 0.9f)]
    [SerializeField] private float brakingStartPercent = 0.5f; // Start braking halfway through hitstun

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;

    private bool isHitStunned = false;
    private float hitstunTimer = 0.3f;
    private float totalHitstunDuration = 0.3f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isHitStunned)
        {
            hitstunTimer -= Time.deltaTime;

            float brakingThreshold = totalHitstunDuration * brakingStartPercent;

            if (hitstunTimer <= brakingThreshold)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x * brakingMultiplier, rb.linearVelocity.y);
            }

            if (hitstunTimer <= 0f)
            {
                isHitStunned = false;
            }
            return;
        }

        // Handle input based on player ID
        GetPlayerInput();

        // Ground Check
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            maxJumps = isGrounded ? 2 : maxJumps; // Reset jumps when grounded
        }

        // Jump Input
        if (playerID == 1)
        {
            if (Input.GetKeyDown(KeyCode.W) && isGrounded)
            {
                Jump();
            }
            else if (Input.GetKeyDown(KeyCode.W) && !isGrounded && maxJumps > 0)
            {
                Jump();
                maxJumps--;
            }
        }
        else if (playerID == 2)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)
            {
                Jump();
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) && !isGrounded && maxJumps > 0)
            {
                Jump();
                maxJumps--;
            }
        }
    }

    private void GetPlayerInput()
    {
        horizontalInput = 0f;

        if (playerID == 1)
        {
            // Player 1 uses A and D
            if (Input.GetKey(KeyCode.A)) horizontalInput = -1f;
            if (Input.GetKey(KeyCode.D)) horizontalInput = 1f;
        }
        else if (playerID == 2)
        {
            // Player 2 uses Left and Right Arrow keys
            if (Input.GetKey(KeyCode.LeftArrow)) horizontalInput = -1f;
            if (Input.GetKey(KeyCode.RightArrow)) horizontalInput = 1f;
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private void FixedUpdate()
    {
        if (isHitStunned) return;

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }
    public void ApplyHitstun(float duration)
    {
        isHitStunned = true;
        totalHitstunDuration = duration;
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