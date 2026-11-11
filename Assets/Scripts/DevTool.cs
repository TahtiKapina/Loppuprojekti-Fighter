using UnityEngine;
using UnityEngine.SceneManagement;

public class DevTool : MonoBehaviour
{
    private void Update()
    {
        RestartGame();
    }

    public void RestartGame()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        
    }
}
