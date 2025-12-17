using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections; // <-- BU KÜTÜPHANE COROUTINE ÝÇÝN ÞART!

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Level Ayarlarý")]
    public int baseTargetAmount = 10;
    public float difficultyMultiplier = 1.5f;

    private int currentLevel = 1;
    private int currentProgress = 0;
    private int targetProgress;

    // --- YENÝ EKLENEN KONTROL ---
    private bool isLevelFinished = false; // Level bitti mi kontrolü
    // ----------------------------

    [Header("UI")]
    public Slider progressBar;
    public TextMeshProUGUI levelText;
    public GameObject winPanel;

    [Header("Referanslar")]
    public Spawner spawner;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        targetProgress = Mathf.RoundToInt(baseTargetAmount * Mathf.Pow(difficultyMultiplier, currentLevel - 1));
    }

    void Start()
    {
        if (progressBar != null)
        {
            progressBar.maxValue = targetProgress;
            progressBar.value = 0;
        }

        if (levelText != null)
        {
            levelText.text = "LEVEL " + currentLevel;
        }

        if (winPanel != null) winPanel.SetActive(false);
    }

    void Update()
    {
        // Geliþtirici Kýsayolu (Reset)
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetProgress();
        }
    }

    public void AddProgress(int amount)
    {
        // Eðer level zaten bittiyse (bekleme süresindeysek) daha fazla iþlem yapma
        if (isLevelFinished) return;

        currentProgress += amount;

        if (progressBar != null)
        {
            progressBar.value = currentProgress;
        }

        // HEDEFE ULAÞTIK MI?
        if (currentProgress >= targetProgress)
        {
            // LevelComplete'i direkt çaðýrmýyoruz!
            // Coroutine baþlatýyoruz.
            StartCoroutine(FinishLevelRoutine());
        }
    }

    // --- YENÝ EKLENEN ZAMANLAYICI ---
    IEnumerator FinishLevelRoutine()
    {
        isLevelFinished = true;

        // --- DÜZELTME BURASI ---
        // Barýn görselini ZORLA %100 yap.
        if (progressBar != null)
        {
            progressBar.value = progressBar.maxValue;
        }
        // -----------------------

        Debug.Log("Hedef tutturuldu, bekleniyor...");

        if (spawner != null) spawner.enabled = false;

        // Barýn dolduðunu ve son patlamalarý izlemek için bekle
        yield return new WaitForSeconds(1.5f);

        LevelComplete();
    }
    // --------------------------------

    void LevelComplete()
    {
        Debug.Log("LEVEL COMPLETED EKRANI AÇILIYOR");

        PlayerPrefs.SetInt("CurrentLevel", currentLevel + 1);
        PlayerPrefs.Save();

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        // Her þey bittikten sonra zamaný durdur
        Time.timeScale = 0f;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public int GetTargetProgress()
    {
        return targetProgress;
    }

    public void ResetProgress()
    {
        PlayerPrefs.SetInt("CurrentLevel", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}