using System.Collections;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Countdown Settings")]
    [SerializeField] private float countdownDuration = 3f;
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("References")]
    [SerializeField] private SceneTestMovement[] playerMovements;
    [SerializeField] private PlayerDamage[] playerDamages;

    public static bool IsMatchStarted { get; private set; } = false;

    private void Start()
    {
        // Find all player scripts in the scene automatically if not assigned
        if (playerMovements == null || playerMovements.Length == 0)
            playerMovements = FindObjectsByType<SceneTestMovement>(FindObjectsSortMode.None);

        if (playerDamages == null || playerDamages.Length == 0)
            playerDamages = FindObjectsByType<PlayerDamage>(FindObjectsSortMode.None);

        StartCoroutine(RunCountdown());
    }

    private IEnumerator RunCountdown()
    {
        IsMatchStarted = false;

        SetPlayersActive(false);

        float timer = countdownDuration;

        while (timer > 0f)
        {
            if (countdownText != null)
            {
                countdownText.text = Mathf.Ceil(timer).ToString();
            }

            yield return null;
            timer -= Time.deltaTime;
        }

        if (countdownText != null)
        {
            countdownText.text = "GO!";
            yield return new WaitForSeconds(0.5f);
            countdownText.text = "";
        }

        SetPlayersActive(true);
        IsMatchStarted = true;

        Debug.Log("Match Started!");
    }

    private void SetPlayersActive(bool isActive)
    {
        foreach (var movement in playerMovements)
        {
            if (movement != null) movement.enabled = isActive;
        }

        foreach (var damage in playerDamages)
        {
            if (damage != null) damage.enabled = isActive;
        }
    }
}