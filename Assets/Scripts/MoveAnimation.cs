using UnityEngine;
using UnityEngine.InputSystem;

public class MoveAnimation : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset playerControlsAsset;
    [SerializeField] private int playerID = 1;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string playingBool = "IsPlaying";

    private InputAction moveAction;
    private InputAction jumpAction;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

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
        jumpAction = playerMap.FindAction("Jump");

        if (moveAction == null || jumpAction == null)
        {
            Debug.LogError($"Could not find 'Move' or 'Jump' in action map '{mapName}'.", this);
            return;
        }

        playerMap.Enable();
    }

    private void Update()
    {
        if (animator == null || moveAction == null || jumpAction == null)
            return;

        Vector2 movement = moveAction.ReadValue<Vector2>();
        bool isPlaying = Mathf.Abs(movement.x) > 0.1f || jumpAction.IsPressed();

        animator.SetBool(playingBool, isPlaying);
    }

    private void OnDisable()
    {
        if (animator != null)
            animator.SetBool(playingBool, false);
    }
}
