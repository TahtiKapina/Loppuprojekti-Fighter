using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("General UI References")]
    [SerializeField] private GameObject WinnerText;

    [Header("Player 1 UI References")]
    [SerializeField] private PlayerDamage player1Stats;
    [SerializeField] private TextMeshProUGUI p1PercentageText;
    [SerializeField] private GameObject[] p1StockIcons;

    [Header("Player 2 UI References")]
    [SerializeField] private PlayerDamage player2Stats;
    [SerializeField] private TextMeshProUGUI p2PercentageText;
    [SerializeField] private GameObject[] p2StockIcons;

    void Update()
    {
        UpdatePlayerUI(player1Stats, p1PercentageText, p1StockIcons);
        UpdatePlayerUI(player2Stats, p2PercentageText, p2StockIcons);
        Winner();
    }

    private void UpdatePlayerUI(PlayerDamage stats, TextMeshProUGUI percentageText, GameObject[] stockIcons)
    {
        if (stats == null) return;

        if (percentageText != null)
        {
            percentageText.text = $"{stats.GetPercentage():F0}%";
        }

        int currentStocks = stats.GetStocks();
        for(int i = 0; i < stockIcons.Length; i++)
        {
            if (stockIcons[i] != null)
            {
                stockIcons[i].SetActive(i < currentStocks);
            }
        }
    }

    private void Winner()
    {
        if (player1Stats.GetStocks() <= 0 && WinnerText.activeSelf == false)
        {
            WinnerText.SetActive(true);
            WinnerText.GetComponent<TextMeshProUGUI>().text = "Player 2 Wins!";
            Debug.Log("Player 2 Wins!");
        }
        else if (player2Stats.GetStocks() <= 0 && WinnerText.activeSelf == false)
        {
            WinnerText.SetActive(true);
            WinnerText.GetComponent<TextMeshProUGUI>().text = "Player 1 Wins!";
            Debug.Log("Player 1 Wins!");
        }
    }
}
