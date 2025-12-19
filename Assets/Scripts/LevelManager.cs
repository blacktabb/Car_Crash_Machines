using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Level Uzunluðu (Rastgelelik)")]
    public int minTargetBase = 8;  // Level 1 için minimum taþ sayýsý
    public int maxTargetBase = 12; // Level 1 için maksimum taþ sayýsý
    public int increasePerLevel = 2; // Her levelde bu aralýk ne kadar kaysýn?

    [Header("Zorluk Ayarlarý")]
    public float hpMultiplier = 1.08f; // %10 yerine %8 artýþ (Daha insaflý)
    public float baseGameSpeed = 1.5f; // Baþlangýç hýzý (Eskiden 2.0 idi)
    public float speedMultiplier = 0.02f; // Her level hýz ne kadar artsýn? (Çok yavaþ artýþ)
    public float maxGameSpeed = 3.0f; // Oyun asla bu hýzdan daha hýzlý olmasýn

    [Header("Ekonomi (Altýn)")]
    public int baseGoldReward = 50;
    public int goldPerLevel = 10;
    public TextMeshProUGUI earnedGoldText;

    private int currentLevel = 1;
    private int currentProgress = 0;
    private int targetProgress;
    private bool isLevelFinished = false;

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

        // --- YENÝ HEDEF BELÝRLEME SÝSTEMÝ (RASTGELE) ---
        // Her levelde min ve max deðerleri biraz artýrýyoruz
        int currentMin = minTargetBase + (currentLevel * 1); // Min daha yavaþ artsýn
        int currentMax = maxTargetBase + (currentLevel * increasePerLevel);

        // Bu aralýktan rastgele bir sayý seçiyoruz
        targetProgress = Random.Range(currentMin, currentMax + 1);

        Debug.Log($"Level {currentLevel} Baþladý. Hedef Taþ Sayýsý: {targetProgress} (Aralýk: {currentMin}-{currentMax})");
        // -----------------------------------------------
    }

    void Start()
    {
        if (progressBar != null)
        {
            progressBar.maxValue = targetProgress;
            progressBar.value = 0;
        }

        // BOSS LEVEL KONTROLÜ (Her 5 levelde bir)
        bool isBossLevel = (currentLevel % 5 == 0);

        if (levelText != null)
        {
            if (isBossLevel) levelText.text = "BOSS LEVEL " + currentLevel;
            else levelText.text = "LEVEL " + currentLevel;

            if (isBossLevel) levelText.color = Color.red;
            else levelText.color = Color.white;
        }

        if (winPanel != null) winPanel.SetActive(false);

        ApplyLevelDifficulty();
    }

    void ApplyLevelDifficulty()
    {
        if (GameManager.Instance != null)
        {
            // Hýz Hesabý: Baz Hýz + (Level * Ufak Artýþ)
            float calculatedSpeed = baseGameSpeed + ((currentLevel - 1) * speedMultiplier);

            // Hýzý Max deðere sabitle (Clamp)
            GameManager.Instance.gameSpeed = Mathf.Clamp(calculatedSpeed, baseGameSpeed, maxGameSpeed);

            Debug.Log($"Oyun Hýzý: {GameManager.Instance.gameSpeed}");
        }
    }

    public int GetHealthMultiplier()
    {
        // Boss Çarpaný: 1.5x (Daha önce 2.0 idi, düþürdük)
        float bossMultiplier = (currentLevel % 5 == 0) ? 1.5f : 1.0f;

        // Üstel deðil, kümülatif artýþ (Mathf.Pow yerine düz çarpým daha kontrollü olabilir ama þimdilik Pow kalsýn)
        // Can artýþýný biraz kýstýk (hpMultiplier = 1.08f tavsiye edilir)
        float multiplier = Mathf.Pow(hpMultiplier, currentLevel - 1);

        // Sonuç 1'den küçük olamaz
        if (multiplier < 1) multiplier = 1;

        return Mathf.RoundToInt(multiplier * bossMultiplier);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) ResetProgress();
    }

    public void AddProgress(int amount)
    {
        if (isLevelFinished) return;
        currentProgress += amount;
        if (progressBar != null) progressBar.value = currentProgress;

        if (currentProgress >= targetProgress)
        {
            StartCoroutine(FinishLevelRoutine());
        }
    }

    IEnumerator FinishLevelRoutine()
    {
        isLevelFinished = true;
        if (progressBar != null) progressBar.value = progressBar.maxValue;

        if (spawner != null) spawner.enabled = false;

        // Sahnedeki taþlarýn bitmesini bekle
        while (GameObject.FindGameObjectsWithTag("Stone").Length > 0)
        {
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(1.0f);
        LevelComplete();
    }

    void LevelComplete()
    {
        int goldEarned = baseGoldReward + (currentLevel * goldPerLevel);

        int totalGold = PlayerPrefs.GetInt("TotalGold", 0);
        PlayerPrefs.SetInt("TotalGold", totalGold + goldEarned);

        PlayerPrefs.SetInt("CurrentLevel", currentLevel + 1);
        PlayerPrefs.Save();

        if (earnedGoldText != null) earnedGoldText.text = "+" + goldEarned + " GOLD";

        if (winPanel != null) winPanel.SetActive(true);
        Time.timeScale = 0f;

        CameraShake shaker = Camera.main.GetComponent<CameraShake>();
        if (shaker != null) shaker.StopShake();
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
        Debug.Log("TÜM VERÝLER SÝLÝNÝYOR... (HARD RESET)");

        // 1. HAFIZAYI SÝL
        // DeleteAll: Altýn, Level, Upgrade'ler... Ne varsa siler.
        PlayerPrefs.DeleteAll();

        // Deðiþikliði diske hemen yaz
        PlayerPrefs.Save();

        // 2. SAHNEYÝ YENÝLE
        // Mevcut sahneyi baþtan yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}