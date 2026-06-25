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

    [Header("Zorluk ve Hız")]
    public float hpMultiplier = 1.08f;
    public float baseGameSpeed = 1.5f;
    public float speedMultiplier = 0.02f;
    public float maxGameSpeed = 3.0f;

    [Header("Ekonomi (Level Sonu Bonusu)")]
    public LevelRewardManager rewardManager; // Inspector'dan ata
    public int playerCurrentHealth; // Oyuncunun o anki canı
    public int playerMaxHealth;     // Oyuncunun maks canı
    public int currentLevelIndex;   // Şu anki level sayısı

    [Header("UI Panelleri")]
    public Slider progressBar;         // Dolum çubuğu
    public GameObject winPanel;
    public GameObject gameOverPanel;

    [Header("YENİ PROGRESS BAR UI")]
    public TextMeshProUGUI levelTitleText; // Üstteki yazı: "Normal Level" / "Boss Level"
    public TextMeshProUGUI progressText;   // Ortadaki yazı: "0 / 150"
    public TextMeshProUGUI levelNumberText; // Kalkan içindeki level sayısı

    [Header("PerkManager")]
    private float nextPerkThreshold = 0.5f;

    public Button nextLevelButton;
    private bool isProcessing = false;

    public MonoBehaviour spawner;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        Debug.Log($"Level {currentLevel} Başlatılıyor...");
    }

    void Start()
    {
        VehicleWeapon.ResetGlobalPerks();
        RestoreSoundState();

        // UI Başlangıç Ayarları
        if (progressBar != null) progressBar.value = 0;

        bool isBossLevel = (currentLevel % 5 == 0);

        // --- YENİ UI GÜNCELLEMELERİ ---

        // 1. Level Başlığı (Normal / Boss)
        if (levelTitleText != null)
        {
            levelTitleText.text = isBossLevel ? "BOSS LEVEL" : "NORMAL LEVEL";
            levelTitleText.color = isBossLevel ? Color.red : Color.white;
        }

        // 2. Kalkan içindeki Level Numarası
        if (levelNumberText != null)
        {
            levelNumberText.text = currentLevel.ToString();
        }

        // 3. İlerleme Yazısı (Başlangıçta 0)
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

    // --- LEVEL GENERATOR BU FONKSİYONU ÇAĞIRACAK ---
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

        // --- YENİ: Toplam taş sayısı belli olunca yazıyı güncelle ---
        UpdateProgressText();
        // -----------------------------------------------------------

        nextPerkThreshold = 0.5f;
        Debug.Log($"Hedef Belirlendi: {totalStoneCount} Taş");
    }



    // --- TAŞLAR KIRILINCA ---
    public void AddProgress(int amount)
    {
        destroyedStoneCount += amount;

        if (progressBar != null)
        {
            progressBar.value = destroyedStoneCount;
        }

        // --- YENİ: Her taş kırıldığında yazıyı güncelle ---
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

    // YENİ YARDIMCI FONKSİYON
    void UpdateProgressText()
    {
        if (progressText != null)
        {
            // Örnek: "15 / 150"
            progressText.text = $"{destroyedStoneCount} / {totalStoneCount}";

            // İstersen sağda kalan, solda kırılan gibi de yapabilirsin.
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

    // ... (Kalan fonksiyonlar aynı: FinishLevelRoutine, LevelComplete, vb.) ...

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
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXState(false);
        }
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

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXState(false);
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // --- BURASI DEĞİŞTİ ---
    public void RestartCurrentLevel()
    {
        Time.timeScale = 1f;

        // Retry yaptığımızda fiyat verilerini sıfırlıyoruz.
        ResetPriceData();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Fiyatları sıfırlayan yardımcı fonksiyon
    // Fiyatları sıfırlayan yardımcı fonksiyon
    private void ResetPriceData()
    {
        // 1. Merge Fiyatını Sıfırla (Senin scriptindeki isim: "MergeCount")
        PlayerPrefs.DeleteKey("MergeCount");

        // 2. Silah Alma Fiyatını Sıfırla (Senin scriptindeki isim: "TotalPurchased")
        PlayerPrefs.DeleteKey("TotalPurchased");

        // Değişiklikleri kaydet
        PlayerPrefs.Save();

        // Not: "TotalGold" anahtarını silmiyoruz, parası cebinde kalsın.
    }
    // -----------------------

        public void NextLevel()
    {
        if (isProcessing) return;

        isProcessing = true;

        if (nextLevelButton != null)
            nextLevelButton.interactable = false;

        int adCounter = PlayerPrefs.GetInt("AdCounter", 0);
        adCounter++;

        if (adCounter >= 2)
        {
            Debug.Log("2 Level geçildi, reklam kontrol ediliyor...");
            
            CrazyGamesManager cgManager = CrazyGamesManager.Instance;
            if (cgManager == null) { cgManager = FindFirstObjectByType<CrazyGamesManager>(); }
            
            if (cgManager != null) { 
                cgManager.ShowMidgameAd(() => { LoadNextSceneLogic(); }); 
                PlayerPrefs.SetInt("AdCounter", 0); 
            } else { 
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

        // Progress bar’ı tekrar sync et
        if (progressBar != null)
            progressBar.value = destroyedStoneCount;

        UpdateProgressText();
    }

    public void ForceFinishLevel()
    {
        // Eğer level zaten bittiyse tekrar çalıştırma
        if (isLevelFinished) return;

        Debug.Log("FİNİŞ ÇİZGİSİ GEÇİLDİ! Level Zorla Bitiriliyor...");

        // Sayacı hileyle %100 yapıyoruz (Görsel olarak tam görünsün diye)
        destroyedStoneCount = totalStoneCount;

        if (progressBar != null)
            progressBar.value = progressBar.maxValue;

        UpdateProgressText(); // Yazıyı da güncelle (örn: 150/150 yap)

        // Normal bitiş rutinini çağır
        StartCoroutine(FinishLevelRoutine());
    }

    public void RestoreSoundState()
    {
        if (AudioManager.Instance != null)
        {
            // PlayerPrefs içinden PauseManager'ın kaydettiği değeri okuruz (1 ise açık, 0 ise kapalı)
            bool isSoundOn = PlayerPrefs.GetInt("Sound", 1) == 1;
            AudioManager.Instance.SetSFXState(isSoundOn);
        }
    }
}



