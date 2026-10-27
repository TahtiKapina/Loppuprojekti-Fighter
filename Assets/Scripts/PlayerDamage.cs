using UnityEngine;
using System.Collections;
public class PlayerDamage : MonoBehaviour
{
    [Header("Player Setup")]
    [SerializeField] private int playerID;

    [Header("Player Stats")]
    [SerializeField] private int stocks = 3;
    [SerializeField] private float currentPercentage = 0f;
    [SerializeField] private Vector3 respawnPoint;

    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private LayerMask playerLayer; // Checks for players
    [SerializeField] private Transform attackPoint; // Empty GameObject

    [Header("Knockback Settings")]
    [SerializeField] private float baseKnockback = 10f;
    [SerializeField] private float percentageMultiplier = 0.05f;
    [SerializeField] private float hitstopDuration = 0.07f;
    [SerializeField] private float hitstunDuration = 0.2f;

    private Rigidbody2D rb;
    private SceneTestMovement movementScript;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movementScript = GetComponent<SceneTestMovement>();
        respawnPoint = transform.position; // Save initial position as default respawn
    }

    private void Update()
    {
        bool attackPressed = false;

        // Assign different keys based on Player ID
        if (playerID == 1 && Input.GetKeyDown(KeyCode.J))
        {
            attackPressed = true;
        }
        else if (playerID == 2 && Input.GetKeyDown(KeyCode.Keypad2)) // Player 2 uses 'Numpad 2' key
        {
            attackPressed = true;
        }

        if (attackPressed)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        // If no attack point is assigned, use the player's own position
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

                // Add an upward bias so they launch diagonally instead of straight sideways
                attackDirection.y += 0.2f;

                // Deal damage and trigger knockback
                enemyDamage.TakeDamage(attackDamage, attackDirection);
                Debug.Log($"{gameObject.name} hit {hit.gameObject.name}!");
            }
        }
    }

    public void TakeDamage(float damageAmount, Vector2 attackDirection)
    {
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
}