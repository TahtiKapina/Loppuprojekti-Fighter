using UnityEngine;

public class AttackTest : MonoBehaviour
{
    [SerializeField] private PlayerDamage targetPlayer;

    private void Update()
    {
        // Press K to deal 15% damage and launch the player UP and RIGHT
        if (Input.GetKeyDown(KeyCode.K) && targetPlayer != null)
        {
            Vector2 launchDirection = new Vector2(1f, 1f); // Diagonal launch
            targetPlayer.TakeDamage(15f, launchDirection);
        }
    }
}