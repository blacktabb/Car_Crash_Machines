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
    public LevelRewardManager rewardManager; // Inspector'dan ata
    public int playerCurrentHealth; // Oyuncunun o anki caný
    public int playerMaxHealth;     // Oyuncunun maks caný
    public int currentLevelIndex;   // Þu anki level sayýsý

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

        if (nextLevelInfoText != null) nextLevelInfoText.text = (currentLevel + 1).ToString();
        if (retryLevelInfoText != null) retryLevelInfoText.text = currentLevel.ToString();
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
        // Ses efekti oynat
        if (AudioManager.Instance != null) AudioManager.Instance.PlayWin();
        // ...

        PlayerPrefs.SetInt("CurrentLevel", currentLevel + 1);
        PlayerPrefs.Save();

        if (winPanel != null) winPanel.SetActive(true);
        rewardManager.ShowReward(currentLevel, playerCurrentHealth, playerMaxHealth);

        Time.timeScale = 0f;
    }

    public void HandleLevelFailed()
    {
        // ses efekti oynat
        if (AudioManager.Instance != null) AudioManager.Instance.PlayLose();
        // ...

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // --- BURASI DEÐÝÞTÝ ---
    public void RestartCurrentLevel()
    {
        Time.timeScale = 1f;

        // Retry yaptýðýmýzda fiyat verilerini sýfýrlýyoruz.
        ResetPriceData();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Fiyatlarý sýfýrlayan yardýmcý fonksiyon
    // Fiyatlarý sýfýrlayan yardýmcý fonksiyon
    private void ResetPriceData()
    {
        // 1. Merge Fiyatýný Sýfýrla (Senin scriptindeki isim: "MergeCount")
        PlayerPrefs.DeleteKey("MergeCount");

        // 2. Silah Alma Fiyatýný Sýfýrla (Senin scriptindeki isim: "TotalPurchased")
        PlayerPrefs.DeleteKey("TotalPurchased");

        // Deðiþiklikleri kaydet
        PlayerPrefs.Save();

        // Not: "TotalGold" anahtarýný silmiyoruz, parasý cebinde kalsýn.
    }
    // -----------------------

    public void NextLevel()
    {
        int adCounter = PlayerPrefs.GetInt("AdCounter", 0);
        adCounter++;

        if (adCounter >= 2)
        {
            Debug.Log("2 Level geçildi, reklam kontrol ediliyor...");
            
            // 1. Önce Instance'ý dene
            CrazyGamesManager cgManager = CrazyGamesManager.Instance;

            // 2. Instance yoksa, sahnede manuel ara (Yedek Plan)
            if (cgManager == null)
            {
                cgManager = FindFirstObjectByType<CrazyGamesManager>();
            }

            // 3. Kontrol ve Çalýþtýrma
            if (cgManager != null)
            {
                cgManager.ShowMidgameAd(() => 
                {
                    LoadNextSceneLogic(); 
                });
                
                PlayerPrefs.SetInt("AdCounter", 0);
            }
            else
            {
                // Reklam yöneticisi sahnede HÝÇ YOKSA oyunu dondurma, direkt geç
                Debug.LogWarning("CrazyGamesManager sahnede bulunamadý! Reklam gösterilmeden devam ediliyor.");
                LoadNextSceneLogic();
            }
        }
        else
        {
            Debug.Log($"Reklam yok. Sayaç: {adCounter}/2");
            PlayerPrefs.SetInt("AdCounter", adCounter);
            PlayerPrefs.Save();
            LoadNextSceneLogic();
        }
    }

    void LoadNextSceneLogic()
    {
        currentLevel++;
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        ResetPriceData();
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
    }
    // -----------------------------

    public void ResumeAfterRevive()
    {
        isLevelFinished = false;

        if (spawner != null)
            spawner.enabled = true;

        // Progress bar’ý tekrar sync et
        if (progressBar != null)
            progressBar.value = destroyedStoneCount;

        UpdateProgressText();
    }

    public void ForceFinishLevel()
    {
        // Eðer level zaten bittiyse tekrar çalýþtýrma
        if (isLevelFinished) return;

        Debug.Log("FÝNÝÞ ÇÝZGÝSÝ GEÇÝLDÝ! Level Zorla Bitiriliyor...");

        // Sayacý hileyle %100 yapýyoruz (Görsel olarak tam görünsün diye)
        destroyedStoneCount = totalStoneCount;

        if (progressBar != null)
            progressBar.value = progressBar.maxValue;

        UpdateProgressText(); // Yazýyý da güncelle (örn: 150/150 yap)

        // Normal bitiþ rutinini çaðýr
        StartCoroutine(FinishLevelRoutine());
    }
}