using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class VehicleStackManager : MonoBehaviour
{
    public static VehicleStackManager Instance;

    [Header("Oyun Durumu")]
    public int baseMaxHealth = 3;
    private int totalMaxHealth;
    private int currentHealth;
    public bool isGameOver = false;
    private bool isMerging = false;

    [Header("Ayarlar")]
    public float carHeight = 0.5f;
    public int maxWeaponCount = 6;
    public List<VehicleWeapon> carStack = new List<VehicleWeapon>();

    [Header("Hasar Ayarları")]
    [SerializeField] private float damageCooldown = 0.5f;
    private bool canTakeDamage = true;

    [Header("ÖNEMLİ: Modeller")]
    public GameObject baseWeaponContainer;
    public GameObject[] weaponModels;

    [Header("Ekonomi (Kayıtlı)")]
    public int money = 0;

    [Header("Fiyatlandırma")]
    public int basePrice = 50;
    public int totalPurchasedCount = 1;
    public TextMeshProUGUI buyButtonText;
    public Button buyButtonComponent;

    [Header("Merge Ayarları")]
    public int mergeBasePrice = 50;
    public int mergePriceIncreaseAmount = 25;
    private int mergeCount = 0;
    public TextMeshProUGUI mergeButtonText;
    public Button mergeButtonComponent;

    [Header("UI & Efektler")]
    public TextMeshProUGUI moneyText;
    public GameObject gameOverPanel;
    public Image[] hearts;
    public Sprite Heart;

    public GameObject hitParticlePrefab;
    public CameraShake cameraShake;
    public GameObject mergeEffectPrefab;

    [Header("Perk Manager")]
    public float tempGoldMultiplier = 1.0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        LoadGameData();
    }

    void Start()
    {
        int hpLevel = PlayerPrefs.GetInt("Upg_MaxHealth", 0);
        totalMaxHealth = baseMaxHealth + hpLevel;
        currentHealth = totalMaxHealth;

        UpdateHealthUI();
        SpawnWeapon();
        UpdateUI();
    }

    void SaveGameData()
    {
        PlayerPrefs.SetInt("TotalGold", money);
        PlayerPrefs.SetInt("MergeCount", mergeCount);
        PlayerPrefs.SetInt("TotalPurchased", totalPurchasedCount);
        PlayerPrefs.Save();
    }

    void LoadGameData()
    {
        money = PlayerPrefs.GetInt("TotalGold", 0);
        mergeCount = PlayerPrefs.GetInt("MergeCount", 0);
        totalPurchasedCount = PlayerPrefs.GetInt("TotalPurchased", 1);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isGameOver || !canTakeDamage)
        {
            if (other.CompareTag("Stone")) Destroy(other.gameObject);
            return;
        }

        if (other.CompareTag("Stone"))
        {
            StartCoroutine(DamageCooldownRoutine());
            TakeDamage(other.gameObject);
        }
    }

    IEnumerator DamageCooldownRoutine()
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(damageCooldown);
        canTakeDamage = true;
    }

    void TakeDamage(GameObject stoneObj)
    {
        currentHealth--;
        UpdateHealthUI();

        if (hitParticlePrefab != null) Instantiate(hitParticlePrefab, stoneObj.transform.position, Quaternion.identity);
        if (cameraShake != null) cameraShake.TriggerShake(0.2f, 0.1f);
        // Çarpan taş yok olacağı için LevelManager'a haber veriyoruz
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.AddProgress(1);
        }

        Destroy(stoneObj);

        if (currentHealth <= 0) GameOver();
    }

    void GameOver()
    {
        isGameOver = true;
        if (LevelManager.Instance != null)
            LevelManager.Instance.HandleLevelFailed();
        else
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    // --- YARDIMCI FONKSİYON: GERÇEK LİMİTİ HESAPLA ---
    // Bu fonksiyon hem Level'in izin verdiği limiti hem de Upgrade limitini kıyaslar.
    // Hangisi küçükse onu döndürür. Böylece UI doğru çalışır.
    int GetEffectiveLimit()
    {
        // 1. Satın alınan kapasite (Upgrade)
        int purchasedLimit = 4 + PlayerPrefs.GetInt("Upg_MaxStack", 0);

        // 2. Levelin tavan yüksekliği (Level Generator'dan gelir)
        int levelLimit = 100; // Varsayılan yüksek değer
        if (LevelGenerator.Instance != null)
        {
            levelLimit = LevelGenerator.Instance.CurrentLevelMaxHeight;
        }

        // 3. Oyunun mutlak sınırı (maxWeaponCount) ile de kıyasla
        // (Hangisi en küçükse o geçerlidir)
        return Mathf.Min(purchasedLimit, levelLimit, maxWeaponCount);
    }

    // --- SATIN ALMA ---
    public void BuyWeapon()
    {
        // Limiti hesapla
        int limit = GetEffectiveLimit();

        // Eğer limit dolduysa alma
        if (carStack.Count >= limit) return;

        if (isMerging) return;

        int currentPrice = GetCurrentPrice();
        if (money >= currentPrice)
        {
            money -= currentPrice;
            totalPurchasedCount++;
            SaveGameData();
            SpawnWeapon();
        }
    }

    void SpawnWeapon()
    {
        if (baseWeaponContainer != null)
        {
            GameObject newCarObj = Instantiate(baseWeaponContainer, transform);
            VehicleWeapon newCarScript = newCarObj.GetComponent<VehicleWeapon>();
            newCarScript.level = 1;
            carStack.Add(newCarScript);
            UpdatePositions();
            UpdateUI();
        }
    }

    // --- MERGE ---
    public void MergeWeapons()
    {
        if (isMerging || carStack.Count < 2) return;
        int mergeCost = GetNextMergeCost();
        if (HasMergeablePair() && money >= mergeCost)
        {
            money -= mergeCost;
            mergeCount++;
            SaveGameData();
            StartCoroutine(MergeProcess());
        }
    }

    IEnumerator MergeProcess()
    {
        for (int i = 0; i < carStack.Count - 1; i++)
        {
            VehicleWeapon bottomCar = carStack[i];
            VehicleWeapon topCar = carStack[i + 1];
            if (bottomCar.level == topCar.level)
            {
                isMerging = true;
                yield return StartCoroutine(topCar.MoveAndMerge(bottomCar.transform.localPosition));
                if (mergeEffectPrefab != null) Instantiate(mergeEffectPrefab, bottomCar.transform.position, Quaternion.identity);
                bottomCar.LevelUp();
                if (topCar != null) Destroy(topCar.gameObject);
                carStack.RemoveAt(i + 1);
                UpdatePositions();
                isMerging = false;
                UpdateUI();
                yield break;
            }
        }
    }

    void UpdatePositions()
    {
        for (int i = 0; i < carStack.Count; i++)
        {
            float targetY = (i * carHeight) + (carHeight * 0.5f);
            carStack[i].transform.localPosition = new Vector3(0, targetY, 0);
        }
    }

    // --- UI GÜNCELLEMELERİ (DÜZELTİLDİ) ---
    public void UpdateUI()
    {
        if (moneyText != null) moneyText.text = money.ToString() + " $";

        // --- BURASI DÜZELTİLDİ ---
        // Artık sadece "maxWeaponCount"a değil, o levelin gerçek sınırına bakıyoruz.
        if (buyButtonComponent != null)
        {
            int currentLimit = GetEffectiveLimit(); // Level ve Upgrade limitini al

            if (carStack.Count >= currentLimit)
            {
                // Sınıra ulaşıldıysa butonu kapat ve MAX yaz
                buyButtonText.text = "MAX";
                buyButtonComponent.interactable = false;
            }
            else
            {
                // Yer varsa fiyatı göster
                int price = GetCurrentPrice();
                buyButtonText.text = "BUY WEAPON " + price + "$";
                buyButtonComponent.interactable = (money >= price);
            }
        }
        // -------------------------

        if (mergeButtonComponent != null)
        {
            int mergeCost = GetNextMergeCost();
            bool canMerge = HasMergeablePair();
            mergeButtonText.text = canMerge ? "MERGE " + mergeCost + "$" : "NO MERGE";
            mergeButtonComponent.interactable = canMerge && (money >= mergeCost) && !isMerging;
        }
    }

    void UpdateHealthUI()
    {
        if (hearts == null) return;

        currentHealth = Mathf.Clamp(currentHealth, 0, totalMaxHealth);

        for (int i = 0; i < hearts.Length; i++)
        {
            // Maksimumdan fazlasını tamamen gizle
            if (i >= totalMaxHealth)
            {
                hearts[i].enabled = false;
                continue;
            }

            // Can varsa kalbi göster, yoksa gizle
            hearts[i].enabled = i < currentHealth;

            if (hearts[i].enabled)
            {
                hearts[i].sprite = Heart;
                hearts[i].color = Color.white;
            }
        }
    }


    public void AddMoney(int amount)
    {
        int finalAmount = Mathf.RoundToInt(amount * tempGoldMultiplier);
        money += finalAmount;
        SaveGameData();
        UpdateUI();
        if (UpgradeManager.Instance != null) UpgradeManager.Instance.UpdateUI();
    }

    public void OnHealthUpgradeBought()
    {
        // 1. Yeni kapasiteyi hesapla (Artık bir fazla)
        int hpLevel = PlayerPrefs.GetInt("Upg_MaxHealth", 0);
        totalMaxHealth = baseMaxHealth + hpLevel;

        // 2. Mevcut canı SADECE 1 ARTIR (Yeni alınan kalp dolu gelir)
        currentHealth++;

        // Güvenlik: Asla limiti aşma (Gerçi aşmaz ama tedbir)
        if (currentHealth > totalMaxHealth) currentHealth = totalMaxHealth;

        // UI'ı güncelle
        UpdateHealthUI();
    }

    // --- YENİ PERK FONKSİYONU: EN DÜŞÜK SİLAHI YÜKSELT ---
    public void UpgradeLowestLevelWeapon()
    {
        if (carStack.Count == 0) return; // Silah yoksa işlem yapma

        VehicleWeapon lowestWeapon = null;
        int minLevel = int.MaxValue; // Kıyaslama için en yüksek sayıdan başlıyoruz

        // 1. Kuleyi tara ve en düşük leveli bul
        foreach (var weapon in carStack)
        {
            // Eğer bu silahın leveli, şu ana kadar bulduğumuz en düşükten de küçükse
            if (weapon.level < minLevel)
            {
                minLevel = weapon.level;
                lowestWeapon = weapon;
            }
        }

        // 2. Eğer bir silah bulduysak onu yükselt
        if (lowestWeapon != null)
        {
            Debug.Log($"Perk Aktif: Level {lowestWeapon.level} silah yükseltiliyor...");

            // Silahın kendi LevelUp fonksiyonunu çağırıyoruz
            // Bu fonksiyon zaten level'i 2 ile çarpıyor ve görseli güncelliyor.
            lowestWeapon.LevelUp();

            // UI güncelle (Merge butonları vb. değişebilir)
            UpdateUI();

            // Otomatik Merge Kontrolü (Opsiyonel)
            // Eğer yükseltme sonrası eşleşme olduysa oyuncu merge butonuna basabilir.
        }
    }

    public void Revive()
    {
        currentHealth = totalMaxHealth;
        isGameOver = false; 
        UpdateHealthUI();
        UpdateUI();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (LevelManager.Instance != null)
            LevelManager.Instance.ResumeAfterRevive();
        Time.timeScale = 1f;
    }
    public void AddHealth()
    {
        currentHealth = totalMaxHealth;
        UpdateHealthUI();
    }


    int GetCurrentPrice() { return basePrice * totalPurchasedCount; }
    int GetNextMergeCost() { return mergeBasePrice + (mergeCount * mergePriceIncreaseAmount); }
    bool HasMergeablePair()
    {
        for (int i = 0; i < carStack.Count - 1; i++)
            if (carStack[i].level == carStack[i + 1].level) return true;
        return false;
    }
    public void GoToMainMenu() { Time.timeScale = 1f; SceneManager.LoadScene("MainMenu"); }
}