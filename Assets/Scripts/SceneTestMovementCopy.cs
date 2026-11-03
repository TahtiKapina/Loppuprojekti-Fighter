using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SceneTestMovementCopy : MonoBehaviour
{
    [Header("Player Setup")]
    [SerializeField] private int playerID; // Player 1 = 1, Player 2 = 2

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private int maxJumps = 1;
    private bool isFacingRight = true;
    public bool IsFacingRight => isFacingRight;

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
    [SerializeField] private InputActionAsset playerControlsAsset;
    private PlayerInput playerInput; // Reference to new Input System component
    private InputAction moveAction;
    private InputAction jumpAction;

    private float horizontalInput;
    private bool isGrounded;

    private bool isHitStunned = false;
    private float hitstunTimer = 0.3f;
    private float totalHitstunDuration = 0.3f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (playerControlsAsset != null)
        {
            // Find the correct Action Map depending on whether this is Player 1 or Player 2
            var mapName = (playerID == 1) ? "Player1": "Player2";
            var playerMap = playerControlsAsset.FindActionMap(mapName);

            if (playerMap != null)
            {
                playerMap.Enable(); // Turn on this player's map
                moveAction = playerMap.FindAction("Move");
                jumpAction = playerMap.FindAction("Jump");

                if (moveAction == null) Debug.LogError("Could not find 'Move' action in map: " + mapName);
                if (jumpAction == null) Debug.LogError("Could not find 'Jump' action in map: " + mapName);
            }
            else
            {
                Debug.LogError("Could not find Action Map: " + mapName);
            }
        }
        else
        {
            Debug.LogError("Player Controls Asset is not assigned on " + gameObject.name);
        }
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

        // Read movement input values from the New Input System
        GetPlayerInput();

        // Ground Check
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            maxJumps = isGrounded ? 2 : maxJumps; // Reset jumps when grounded
        }

        // Jump Input
        if (jumpAction != null && jumpAction.WasPressedThisFrame())
        {
            if (isGrounded)
            {
                Jump();
            }
            else if (!isGrounded && maxJumps > 0)
            {
                Jump();
                maxJumps--;
            }
        }
    }

    private void GetPlayerInput()
    {
        if (moveAction != null)
        {
            // Read the 2D Vector (X = Left/Right, Y = Up/Down) from Move action
            Vector2 moveVector = moveAction.ReadValue<Vector2>();
            horizontalInput = moveVector.x;

            // Track facing direction
            if (horizontalInput < -0.1f)
            {
                isFacingRight = false;
            }
            else if (horizontalInput > 0.1f)
            {
                isFacingRight = true;
            }
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