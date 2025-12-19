using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI levelInfoText;
    public TextMeshProUGUI totalGoldText; // --- YENÝ ---

    void Start()
    {
        int savedLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        int savedGold = PlayerPrefs.GetInt("TotalGold", 0); // --- YENÝ ---

        if (levelInfoText != null)
        {
            levelInfoText.text = "LEVEL " + savedLevel;
        }

        // --- YENÝ ---
        if (totalGoldText != null)
        {
            totalGoldText.text = savedGold.ToString() + " G"; // Yanýna G veya altýn ikonu koyabilirsin
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene");
    }
}