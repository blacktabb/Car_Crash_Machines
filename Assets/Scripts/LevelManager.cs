using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Level Bilgileri")]
    public int currentLevel = 1;
    private int totalStoneCount = 0;
    private int destroyedStoneCount = 0;
    private bool isLevelFinished = false;

    [Header("Bilgilendirme UI")]
    public TextMeshProUGUI nextLevelInfoText;
    public TextMeshProUGUI retryLevelInfoText;

    [Header("Zorluk ve Hýz")]
    public float hpMultiplier = 1.08f;
    public float baseGameSpeed = 1.5f;
    public float speedMultiplier = 0.02f;
    public float maxGameSpeed = 3.0f;

    [Header("Ekonomi (Level Sonu Bonusu)")]
    public int baseGoldReward = 100;
    public int goldPerLevel = 10;
    public TextMeshProUGUI earnedGoldText;

    [Header("UI Panelleri")]
    public Slider progressBar;         // Dolum çubuðu
    public GameObject winPanel;
    public GameObject gameOverPanel;

    [Header("YENÝ PROGRESS BAR UI")]
    public TextMeshProUGUI levelTitleText; // Üstteki yazý: "Normal Level" / "Boss Level"
    public TextMeshProUGUI progressText;   // Ortadaki yazý: "0 / 150"
    public TextMeshProUGUI levelNumberText; // Kalkan içindeki level sayýsý

    [Header("PerkManager")]
    private float nextPerkThreshold = 0.5f;

    public MonoBehaviour spawner;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        Debug.Log($"Level {currentLevel} Baþlatýlýyor...");
    }

    void Start()
    {
        // UI Baþlangýç Ayarlarý
        if (progressBar != null) progressBar.value = 0;

        bool isBossLevel = (currentLevel % 5 == 0);

        // --- YENÝ UI GÜNCELLEMELERÝ ---

        // 1. Level Baþlýðý (Normal / Boss)
        if (levelTitleText != null)
        {
            levelTitleText.text = isBossLevel ? "BOSS LEVEL" : "NORMAL LEVEL";
            levelTitleText.color = isBossLevel ? Color.red : Color.white;
        }

        // 2. Kalkan içindeki Level Numarasý
        if (levelNumberText != null)
        {
            levelNumberText.text = currentLevel.ToString();
        }

        // 3. Ýlerleme Yazýsý (Baþlangýçta 0)
        if (progressText != null)
        {
            progressText.text = "0 / " + totalStoneCount; // Geçici, SetLevelTarget'ta güncellenecek
        }

        // ------------------------------

        if (winPanel != null) winPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (nextLevelInfoText != null) nextLevelInfoText.text = "LEVEL " + (currentLevel + 1);
        if (retryLevelInfoText != null) retryLevelInfoText.text = "LEVEL " + currentLevel;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetProgress();
        }
    }

    // --- LEVEL GENERATOR BU FONKSÝYONU ÇAÐIRACAK ---
    public void SetLevelTarget(int amount)
    {
        totalStoneCount = amount;
        destroyedStoneCount = 0;
        isLevelFinished = false;

        if (progressBar != null)
        {
            progressBar.maxValue = totalStoneCount;
            progressBar.value = 0;
        }

        // --- YENÝ: Toplam taþ sayýsý belli olunca yazýyý güncelle ---
        UpdateProgressText();
        // -----------------------------------------------------------

        nextPerkThreshold = 0.5f;
        Debug.Log($"Hedef Belirlendi: {totalStoneCount} Taþ");
    }

    // --- TAÞLAR KIRILINCA ---
    public void AddProgress(int amount)
    {
        destroyedStoneCount += amount;

        if (progressBar != null)
        {
            progressBar.value = destroyedStoneCount;
        }

        // --- YENÝ: Her taþ kýrýldýðýnda yazýyý güncelle ---
        UpdateProgressText();
        // --------------------------------------------------

        // Perk Sistemi Kontrolü
        float progressPercent = (float)destroyedStoneCount / (float)totalStoneCount;

        if (progressPercent >= nextPerkThreshold && progressPercent < 0.95f)
        {
            if (PerkManager.Instance != null) PerkManager.Instance.ShowPerkSelection();
            nextPerkThreshold += 0.5f;
        }

        if (destroyedStoneCount >= totalStoneCount && !isLevelFinished)
        {
            StartCoroutine(FinishLevelRoutine());
        }
    }

    // YENÝ YARDIMCI FONKSÝYON
    void UpdateProgressText()
    {
        if (progressText != null)
        {
            // Örnek: "15 / 150"
            progressText.text = $"{destroyedStoneCount} / {totalStoneCount}";

            // Ýstersen saðda kalan, solda kýrýlan gibi de yapabilirsin.
            // Örnek: progressText.text = $"{destroyedStoneCount}           {totalStoneCount - destroyedStoneCount}";
        }
    }

    public int GetHealthMultiplier()
    {
        float bossMultiplier = (currentLevel % 5 == 0) ? 1.5f : 1.0f;
        float multiplier = Mathf.Pow(hpMultiplier, currentLevel - 1);
        if (multiplier < 1) multiplier = 1;
        return Mathf.RoundToInt(multiplier * bossMultiplier);
    }

    // ... (Kalan fonksiyonlar ayný: FinishLevelRoutine, LevelComplete, vb.) ...

    IEnumerator FinishLevelRoutine()
    {
        isLevelFinished = true;

        if (progressBar != null) progressBar.value = progressBar.maxValue;
        if (spawner != null) spawner.enabled = false;

        yield return new WaitForSeconds(1.0f);
        LevelComplete();
    }

    void LevelComplete()
    {
        int levelBonus = baseGoldReward + (currentLevel * goldPerLevel);

        if (VehicleStackManager.Instance != null)
        {
            VehicleStackManager.Instance.AddMoney(levelBonus);
        }

        if (earnedGoldText != null)
            earnedGoldText.text = "+" + levelBonus + " GOLD (Bonus)";

        PlayerPrefs.SetInt("CurrentLevel", currentLevel + 1);
        PlayerPrefs.Save();

        if (winPanel != null) winPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void HandleLevelFailed()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RestartCurrentLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
    }

    public void ResetProgress()
    {
        Debug.Log("TÜM ÝLERLEME SÝLÝNÝYOR... SIFIRDAN BAÞLATILIYOR.");
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}