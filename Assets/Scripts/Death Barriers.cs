using UnityEngine;

public class DeathBarriers : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerDamage player = collision.GetComponent<PlayerDamage>();

        if (player != null)
        {
            player.LoseStock();
            Debug.Log("Player has lost a stock!");
        }
    }
}
