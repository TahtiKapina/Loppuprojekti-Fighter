using UnityEngine;
using UnityEngine.InputSystem;

public class MoveAnimation : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset playerControlsAsset;
    [SerializeField] private int playerID = 1;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string movingLeftBool = "MovingLeft";
    [SerializeField] private string movingRightBool = "MovingRight";
    [SerializeField] private string jumpingBool = "Jumping";
    [SerializeField] private string fallingBool = "Falling";
    [SerializeField] private string landingTrigger = "Landing";
    [SerializeField] private string moveBlendParameter = "MoveGround";
    [SerializeField, Min(0f)] private float movementDeadzone = 0.1f;

    [SerializeField, Min(0.01f)] private float maxFallSpeedForBlend = 14f;
    [SerializeField, Min(0.01f)] private float maxRiseSpeedForBlend = 14f;

    private InputAction moveAction;
    private SceneTestMovement playerMovement;
    private Rigidbody2D playerRigidbody;

    private bool hasGroundState;
    private bool wasGrounded;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        playerMovement = GetComponent<SceneTestMovement>();
        playerRigidbody = GetComponent<Rigidbody2D>();

        if (playerMovement == null)
            Debug.LogError("SceneTestMovement was not found on this player.", this);

        if (playerRigidbody == null)
            Debug.LogError("Rigidbody2D was not found on this player.", this);

        if (playerControlsAsset == null)
        {
            Debug.LogError("Player Controls Asset is not assigned.", this);
            return;
        }

        string mapName = playerID == 1 ? "Player1" : "Player2";
        InputActionMap playerMap = playerControlsAsset.FindActionMap(mapName);

        if (playerMap == null)
        {
            Debug.LogError($"Could not find action map '{mapName}'.", this);
            return;
        }

        moveAction = playerMap.FindAction("Move");

        if (moveAction == null)
        {
            Debug.LogError($"Could not find 'Move' in action map '{mapName}'.", this);
            return;
        }

        playerMap.Enable();
    }

    private void Update()
    {
        if (animator == null || playerMovement == null || playerRigidbody == null)
            return;

        float verticalVelocity = playerRigidbody.linearVelocity.y;

        // Treat upward motion as airborne even if the ground check still overlaps.
        bool isGrounded = playerMovement.IsGrounded && verticalVelocity <= 0.1f;

        Vector2 movement = moveAction != null
            ? moveAction.ReadValue<Vector2>()
            : Vector2.zero;

        bool directionLocked = playerMovement.IsJumpDirectionLocked;
        float horizontalInput = directionLocked
            ? playerMovement.PendingJumpDirection
            : Mathf.Clamp(movement.x, -1f, 1f);

        if (Mathf.Abs(horizontalInput) < movementDeadzone)
            horizontalInput = 0f;

        bool movingLeft = isGrounded && !directionLocked && horizontalInput < 0f;
        bool movingRight = isGrounded && !directionLocked && horizontalInput > 0f;
        bool jumping = !isGrounded && verticalVelocity > 0f;
        bool falling = !isGrounded && verticalVelocity <= 0f;

        if (playerMovement.JumpPressed)
        {
            if (isGrounded)
                animator.SetTrigger("JumpTrigger");
            else
                animator.SetTrigger("JumpInAirTrigger");

            playerMovement.JumpPressed = false;
        }

        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool(movingLeftBool, movingLeft);
        animator.SetBool(movingRightBool, movingRight);
        animator.SetBool(jumpingBool, jumping);
        animator.SetBool(fallingBool, falling);

        // Blend tree values: 0 = left, 0.5 = in place, 1 = right.
        animator.SetFloat(moveBlendParameter, (horizontalInput + 1f) * 0.5f);
        animator.SetFloat("Y_Velocity", playerRigidbody.linearVelocity.y);

        float verticalSpeed = playerRigidbody.linearVelocity.y;
        float jumpBlend = Mathf.InverseLerp(
            -maxFallSpeedForBlend,
            maxRiseSpeedForBlend,
            verticalSpeed);

        animator.SetFloat("VerticalSpeed", jumpBlend);

        if (hasGroundState && !wasGrounded && isGrounded)
            animator.SetTrigger(landingTrigger);

        wasGrounded = isGrounded;
        hasGroundState = true;
    }

    private void OnDisable()
    {
        if (animator == null)
            return;

        animator.SetBool(movingLeftBool, false);
        animator.SetBool(movingRightBool, false);
        animator.SetBool(jumpingBool, false);
        animator.SetBool(fallingBool, false);
        animator.ResetTrigger(landingTrigger);
    }
}
