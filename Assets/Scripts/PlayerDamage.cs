using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerDamage : MonoBehaviour
{
    [Header("Player Setup")]
    [SerializeField] private int playerID; // Player 1 = 1, Player 2 = 2

    [Header("Player Stats")]
    [SerializeField] public int stocks = 3;
    [SerializeField] public float currentPercentage = 0f;
    [SerializeField] private Vector3 respawnPoint;

    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private LayerMask playerLayer; // Checks for players
    [SerializeField] private Transform attackPoint; // Empty GameObject

    [Header("Knockback Settings")]
    [SerializeField] private float baseKnockback = 10f;
    [SerializeField] private float percentageMultiplier = 0.05f;
    [Header("Immunity Settings")]
    [SerializeField] private float hitstopDuration = 0.07f;
    [SerializeField] private float hitstunDuration = 0.2f;
    [SerializeField] private float immunityDuration = 3f;
    [SerializeField] private float flashInterval = 0.15f;

    private bool isImmune = false;
    private SpriteRenderer spriteRenderer;

    [Header("Attack Offsets")]
    [SerializeField] private Vector3 sideOffset = new Vector3(1f, 0f, 0f);
    [SerializeField] private Vector3 upOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private Vector3 downOffset = new Vector3(0f, -1f, 0f);
    [SerializeField] private Vector3 neutralOffset = new Vector3(0.5f, 0f, 0f);

    private Rigidbody2D rb;
    private SceneTestMovement movementScript;
    [SerializeField] private InputActionAsset playerControlsAsset;
    private InputAction moveAction;
    private InputAction attackAction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movementScript = GetComponent<SceneTestMovement>();
        respawnPoint = transform.position; // Save initial position as default respawn
 
        spriteRenderer = GetComponent<SpriteRenderer>();


        if (playerControlsAsset != null)
        {
            var mapName = (playerID == 1) ? "Player1" : "Player2";
            var playerMap = playerControlsAsset.FindActionMap(mapName);

            if (playerMap != null)
            {
                playerMap.Enable();

                attackAction = playerMap.FindAction("Attack");
                moveAction = playerMap.FindAction("Move");
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
        UpdateAttackPointPosition();

        if (attackAction != null && attackAction.triggered)
        {
            PerformAttack();         
        }
    }

    private void UpdateAttackPointPosition()
    {
        if (attackPoint == null) return;

        Vector3 targetOffset = neutralOffset;

        if (moveAction != null)
        {
            Vector2 moveInput = moveAction.ReadValue<Vector2>();
            float horizontal = moveInput.x;
            float vertical = moveInput.y;

            // Determine offset based on where the stick is pointing right now
            if (vertical > 0.5f)
            {
                targetOffset = upOffset;
            }
            else if (vertical < -0.5f)
            {
                targetOffset = downOffset;
            }
            else if (Mathf.Abs(horizontal) > 0.3f)
            {
                targetOffset = sideOffset;
                // Face the offset in the direction of the horizontal input
                targetOffset.x = Mathf.Sign(horizontal) * Mathf.Abs(sideOffset.x);
            }
            else
            {
                // Neutral: Default to facing direction based on character scale
                float facingDir = Mathf.Sign(transform.localScale.x != 0 ? transform.localScale.x : 1f);
                targetOffset = neutralOffset;
                targetOffset.x = Mathf.Abs(neutralOffset.x) * facingDir;
            }
        }

        // Apply the position smoothly or instantly to the child object
        attackPoint.localPosition = targetOffset;
    }

    private void PerformAttack()
    {
        Debug.Log($"Player {playerID} performed an attack!");

        // Set the attackPoint's local position based on the calculated offset
        Vector2 checkPos = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;

        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(checkPos, attackRange, playerLayer);

        foreach (Collider2D hit in hitPlayers)
        {
            // You don't get hit
            if (hit.gameObject == gameObject) continue;

            // Check for PlayerDamage script
            PlayerDamage enemyDamage = hit.GetComponent<PlayerDamage>();
            if (enemyDamage != null)
            {
                // Calculate knockback based on attakers position
                Vector2 attackDirection = (hit.transform.position - transform.position).normalized;

                // upward bias
                attackDirection.y += 0.2f;

                // Deal damage and trigger knockback
                enemyDamage.TakeDamage(attackDamage, attackDirection);
                Debug.Log($"{gameObject.name} hit {hit.gameObject.name}!");
            }
        }
    }

    public void TakeDamage(float damageAmount, Vector2 attackDirection)
    {
        Debug.Log("attack recieved. immune:" + isImmune);
        if (isImmune)
        {
            Debug.Log("Attack blocked");
            return;
        }
        // Add damage %
        currentPercentage += damageAmount;
        Debug.Log($"Player hit! Current Damage: {currentPercentage}%");

        float calculatedForce = baseKnockback + (currentPercentage * percentageMultiplier);

        rb.linearVelocity = Vector2.zero;

        StartCoroutine(HitstopAndLaunch(attackDirection, calculatedForce));
    }

    private IEnumerator HitstopAndLaunch(Vector2 direction, float force)
    {
        // 1. Freeze movement script and physics simulation completely for a split second
        movementScript.ApplyHitstun(hitstopDuration + hitstunDuration); // Keep input locked
        rb.simulated = false;

        // Wait in real-time for the hitstop freeze effect
        yield return new WaitForSecondsRealtime(hitstopDuration);

        // 2. Unfreeze physics
        rb.simulated = true;

        // 3. Removes momentum from player
        rb.linearVelocity = Vector2.zero;

        // 4. Apply the knockback
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);

        // 5. Allow hitstun flight timer to finish counting down
        yield return new WaitForSecondsRealtime(hitstunDuration);
    }

    public void LoseStock()
    {
        stocks--;
        Debug.Log($"Stock lost! Stocks remaining: {stocks}");

        if (stocks > 0)
        {
            Respawn();
            // TODO: Perks for the player losing the stock
        }
        else
        {
            Debug.Log("Game Over!");
            gameObject.SetActive(false); // Hide player
        }
    }

    private void Respawn()
    {
        currentPercentage = 0f; // Reset damage
        rb.linearVelocity = Vector2.zero; // Clear momentum
        transform.position = respawnPoint; // Move back to spawn position

        Debug.Log("Respawn called - starting immunity frames");

        StartCoroutine(ImmunityFrames());
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 drawPos = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(drawPos, attackRange);
    }

    // Helper getters for UI display
    public float GetPercentage() => currentPercentage;
    public int GetStocks() => stocks;
    private IEnumerator ImmunityFrames()
    {
        isImmune = true;

        Debug.Log("IMMUNITY ACTIVATED!");

        float timer = 0f;

        while (timer < immunityDuration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }

            yield return new WaitForSeconds(flashInterval);

            timer += flashInterval;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        isImmune = false;

        Debug.Log("IMMUNITY ENDED!");
    }
}