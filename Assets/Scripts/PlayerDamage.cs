using UnityEngine;
public class PlayerDamage : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private int stocks = 3;
    [SerializeField] private float currentPercentage = 0f;
    [SerializeField] private Vector3 respawnPoint;

    [Header("Knockback Settings")]
    [SerializeField] private float baseKnockback = 10f;
    [SerializeField] private float percentageMultiplier = 0.05f;
    [SerializeField] private float hitstunDuration = 0.2f;

    private Rigidbody2D rb;
    private SceneTestMovement movementScript;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movementScript = GetComponent<SceneTestMovement>();
        respawnPoint = transform.position; // Save initial position as default respawn
    }

    public void TakeDamage(float damageAmount, Vector2 attackDirection)
    {
        // Add damage %
        currentPercentage += damageAmount;
        Debug.Log($"Player hit! Current Damage: {currentPercentage}%");

        float calculatedForce = baseKnockback + (currentPercentage * percentageMultiplier);

        rb.linearVelocity = Vector2.zero;

        // Apply knockback impulse force
        rb.AddForce(attackDirection.normalized * calculatedForce, ForceMode2D.Impulse);

        movementScript.ApplyHitstun(hitstunDuration);
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
            gameObject.SetActive(false); // Hide player on final death
        }
    }

    private void Respawn()
    {
        currentPercentage = 0f; // Reset damage %
        rb.linearVelocity = Vector2.zero; // Clear momentum
        transform.position = respawnPoint; // Move back to spawn position
    }

    // Helper getters for UI display
    public float GetPercentage() => currentPercentage;
    public int GetStocks() => stocks;
}