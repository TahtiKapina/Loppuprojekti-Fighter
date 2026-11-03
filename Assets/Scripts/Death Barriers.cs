using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathBarriers : MonoBehaviour
{
    public GameObject restartButton;

    private bool gameOverTriggered = false;

    private void Start()
    {
        restartButton.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerDamage player = collision.GetComponent<PlayerDamage>();

        if (player != null)
        {
            player.LoseStock();

            Debug.Log("Player has lost a life!");

            // Check if player has lost all 3 lives
            if (!gameOverTriggered && player.stocks <= 0)
            {
                gameOverTriggered = true;

                Debug.Log("GAME OVER!");

               
                restartButton.SetActive(true);
            }
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}