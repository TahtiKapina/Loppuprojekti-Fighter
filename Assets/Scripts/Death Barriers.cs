using UnityEngine;

public class DeathBarriers : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerDamage player = GetComponent<PlayerDamage>();

        if (player != null)
        {
            player.LoseStock();
        }
    }
}
