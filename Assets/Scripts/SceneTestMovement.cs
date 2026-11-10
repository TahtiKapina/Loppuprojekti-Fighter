using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SceneTestMovement : MonoBehaviour
{
    [Header("Player Setup")]
    [SerializeField] private int playerID; // Player 1 = 1, Player 2 = 2

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private int maxJumps = 1;
    public bool JumpPressed { get; set; } = false;
    private bool isFacingRight = true;
    public bool IsFacingRight => isFacingRight;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Knockback Braking Feel")]
    [Range(0.1f, 0.98f)]
    [SerializeField] private float brakingMultiplier = 0.85f; // Lower = harder/faster brakes
    [Range(0.1f, 0.99f)]
    [SerializeField] private float brakingStartPercent = 0.5f; // Start braking halfway through hitstun

    [Header("Ledge Grab Settings")]
    [SerializeField] private Transform ledgeCheck;
    [SerializeField] private LayerMask ledgeLayer;
    [SerializeField] private float ledgeCheckRadius = 0.15f;
    [SerializeField] private float ledgeCooldownDuration = 0.5f;
    private float ledgeCooldownTimer = 0f;
    private bool isLedgeGrabbed = false;
    private bool wasLedgeGrabbed = false;
    private Vector2 grabdir;
    private Transform currentLedge;

    private Rigidbody2D rb;
    [SerializeField] private InputActionAsset playerControlsAsset;
    private PlayerInput playerInput;
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
            var mapName = (playerID == 1) ? "Player1" : "Player2";
            var playerMap = playerControlsAsset.FindActionMap(mapName);

            if (playerMap != null)
            {
                playerMap.Enable(); // Turn on this player's map
                moveAction = playerMap.FindAction("Move");
                jumpAction = playerMap.FindAction("Jump");

                if (moveAction == null) Debug.LogError("Could not find 'Move' action in map: " + mapName);
                if (jumpAction == null) Debug.LogError("Could not find 'Jump' action in map: " + mapName);

                if (Gamepad.all.Count >= playerID)
                {
                    // Player 1 gets Gamepad.all[0], Player 2 gets Gamepad.all[1]
                    InputDevice targetDevice = Gamepad.all[playerID - 1];

                    if (playerID == 1 && Keyboard.current != null)
                    {
                        playerMap.devices = new InputDevice[] { targetDevice, Keyboard.current };
                    }
                    else
                    {
                        // Player 2 gets their specific gamepad
                        playerMap.devices = new InputDevice[] { targetDevice };
                    }
                }
                else
                {
                    // if player 1 has no gamepad, allow keyboard; 
                    if (playerID == 1 && Keyboard.current != null)
                    {
                        playerMap.devices = new InputDevice[] { Keyboard.current };
                    }
                    else if (playerID == 2 && Keyboard.current != null)
                    {
                        // Player 2 falls back to keyboard if only 1 controller is plugged in (or 0)
                        playerMap.devices = new InputDevice[] { Keyboard.current };
                        Debug.Log("Player 2 falling back to Keyboard controls.");
                    }
                    else
                    {
                        // Player 2 has no second controller, so keep them disabled
                        playerMap.devices = new InputDevice[0];
                        playerMap.Disable();
                        Debug.LogWarning($"Not enough gamepads connected for Player {playerID}. Controls disabled.");
                    }
                }
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


        if (isLedgeGrabbed)
        {
            rb.linearVelocity = Vector2.zero;

            Vector2 currentMoveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
            float vertical = currentMoveInput.y;

            if (vertical > 0.5f || (jumpAction != null && jumpAction.WasPressedThisFrame()))
            {
                ClimbUpLedge();
            }
            else if(Input.GetKeyDown(KeyCode.S))
            {
                ReleaseLedge();
            }
        }

        if (ledgeCooldownTimer > 0f)
        {
            ledgeCooldownTimer -= Time.deltaTime;
        }

        isLedgeGrabbed = false;

        if (!isGrounded && ledgeCheck != null && ledgeCooldownTimer <= 0f)
        {
            Collider2D ledgeHit = Physics2D.OverlapCircle(ledgeCheck.position, ledgeCheckRadius, ledgeLayer);
            if (ledgeHit != null)
            {
                Vector2 closestPoint = ledgeHit.ClosestPoint(rb.position); // Optional: Get the closest point on the ledge collider to the player
                Vector2 Toclosest = closestPoint - rb.position; // Optional: Calculate the direction to the ledge
                float dot = Vector2.Dot(Toclosest.normalized, Vector2.up); // Optional: Check if the ledge is above the player
                if (dot < 0.5f)
                {
                    GrabLedge(ledgeHit.transform);
                    grabdir = new Vector2(Toclosest.x, 0).normalized; // Store the horizontal direction to the ledge for later use
                }
            }
        }

        if (wasLedgeGrabbed && !isLedgeGrabbed)
        {
            // Player has just released the ledge, start cooldown
            rb.linearVelocity = grabdir * 2f; // Optional: Apply a small push away from the ledge
        }

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

        if (ledgeCheck == null)
        {
            Debug.LogWarning("Ledge Check Transform is not assigned on " + gameObject.name);
        }

        wasLedgeGrabbed = isLedgeGrabbed;
    }

    private void GrabLedge(Transform ledgeTransform)
    {
        if(isLedgeGrabbed == false)
        {

        }
        // rb.linearVelocity = Vector2.zero;
        isLedgeGrabbed = true;
        currentLedge = ledgeTransform;
        rb.simulated = false;


        Debug.Log("Grabbed ledge!");
    }

    private void ClimbUpLedge()
    {
        rb.simulated = true;

        // TODO: Shift player position slightly forward and upward onto the platform surface
        rb.MovePosition(rb.position + new Vector2(0, 7.5f) * Time.deltaTime);
    }

    private void ReleaseLedge()
    {
        isLedgeGrabbed = false;
        wasLedgeGrabbed = false;
        rb.simulated = true;
        ledgeCooldownTimer = ledgeCooldownDuration;
        rb.linearVelocity = new Vector2(0f, -2f);
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
        JumpPressed = true;
    }
    public void JumpFromAnimation (int unused)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private void FixedUpdate()
    {
        if (isHitStunned) return;

        float targetSpeed = horizontalInput * moveSpeed;

        if (Mathf.Abs(rb.linearVelocity.x) > moveSpeed)
        {
            rb.linearDamping = 8.0f;
            return;
        }
        else
        {
            rb.linearDamping = 0.05f;
        }

        rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
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
        // Visual indicator in Scene view for Ledge Check
        if (ledgeCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(ledgeCheck.position, ledgeCheckRadius);
        }
    }

    public bool IsGrounded => isGrounded;
}