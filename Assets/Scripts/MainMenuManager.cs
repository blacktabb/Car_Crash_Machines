using UnityEngine;
using UnityEngine.SceneManagement; // Sahne deðiþimi için þart
using TMPro; // TextMeshPro için þart

public class MainMenuManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI levelInfoText; // "Current Level: 5" yazýsý

    void Start()
    {
        // 1. Kayýtlý Leveli Çek
        // Eðer kayýt yoksa varsayýlan olarak 1 döner.
        int savedLevel = PlayerPrefs.GetInt("CurrentLevel", 1);

        // 2. Ekrana Yazdýr
        if (levelInfoText != null)
        {
            levelInfoText.text = "LEVEL " + savedLevel;
        }
    }

    // PLAY Butonuna baðlayacaðýmýz fonksiyon
    public void PlayGame()
    {
        // DÝKKAT: Oyun sahnennin adý neyse buraya aynýsýný yazmalýsýn.
        // Senin ekran görüntünde "SampleScene" yazýyordu, o yüzden onu yazdým.
        SceneManager.LoadScene("SampleScene");
    }
}