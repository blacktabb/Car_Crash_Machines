using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class VehicleStackManager : MonoBehaviour
{
    [Header("Oyun Durumu")]
    public int maxHealth = 3;
    private int currentHealth;
    public bool isGameOver = false;
    private bool isMerging = false;

    [Header("Ayarlar")]
    public GameObject baseCarPrefab;
    public float carHeight = 1.0f;
    public int maxWeaponCount = 6;

    public List<VehicleWeapon> carStack = new List<VehicleWeapon>();

    [Header("Ekonomi")]
    public int money = 1000;
    public int currentLevelMoney = 150;

    [Header("Satın Alma Ayarları")]
    public int basePrice = 50;
    public TextMeshProUGUI buyButtonText;
    public Button buyButtonComponent;

    [Header("Merge (Birleştirme) Ayarları")]
    public int mergeBasePrice = 50; // Başlangıç fiyatı
    public int mergePriceIncreaseAmount = 2; // Her kullanımda +2 coin ekle
    private int mergeCount = 0;

    public TextMeshProUGUI mergeButtonText;
    public Button mergeButtonComponent;

    [Header("UI Genel")]
    public TextMeshProUGUI moneyText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI healthText;

    [Header("Hasar Ayarı")]
    public float damageCooldown = 0.5f;
    private float nextDamageTime = 0f;

    [Header("Efektler")]
    public GameObject hitParticlePrefab;
    public CameraShake cameraShake;
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.1f;

    void Start()
    {
        // --- DEĞİŞİKLİK BURADA ---
        // Eskiden: currentHealth = maxHealth;

        // Şimdi: Hafızadan HP Levelini oku
        int hpLevel = PlayerPrefs.GetInt("Upg_MaxHealth", 0);

        // Yeni Max Health = Baz Can + Upgrade Leveli
        // Örn: Baz 3 + Level 2 = 5 Can
        int finalMaxHealth = maxHealth + hpLevel;

        currentHealth = finalMaxHealth;
        // -------------------------

        money = currentLevelMoney;
        mergeCount = 0;

        UpdateHealthUI(); // Kalpleri ekrana bas
        SpawnWeapon();
        UpdateUI();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isGameOver) return;

        if (other.CompareTag("Stone"))
        {
            if (Time.time < nextDamageTime)
            {
                if (LevelManager.Instance != null) LevelManager.Instance.AddProgress(1);

                float hitX = other.transform.position.x;
                if (hitParticlePrefab != null) Instantiate(hitParticlePrefab, other.transform.position, Quaternion.identity);

                Destroy(other.gameObject);
                ClearColumn(hitX);
                return;
            }

            nextDamageTime = Time.time + damageCooldown;
            TakeDamage(other.gameObject);
        }
    }

    void TakeDamage(GameObject stoneObj)
    {
        currentHealth--;
        UpdateHealthUI();

        if (hitParticlePrefab != null) Instantiate(hitParticlePrefab, stoneObj.transform.position, Quaternion.identity);
        if (cameraShake != null) cameraShake.TriggerShake(shakeDuration, shakeMagnitude);
        if (LevelManager.Instance != null) LevelManager.Instance.AddProgress(1);

        float hitX = stoneObj.transform.position.x;
        Destroy(stoneObj);
        ClearColumn(hitX);

        if (currentHealth <= 0) GameOver();
    }

    void ClearColumn(float collisionX)
    {
        Vector2 startPoint = new Vector2(collisionX, transform.position.y);
        RaycastHit2D[] hits = Physics2D.BoxCastAll(startPoint, new Vector2(3f, 0.1f), 0f, Vector2.up, 20f);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.CompareTag("Stone"))
            {
                if (LevelManager.Instance != null) LevelManager.Instance.AddProgress(1);
                Destroy(hit.collider.gameObject);
            }
        }
    }

    void GameOver()
    {
        isGameOver = true;
        if (cameraShake != null) cameraShake.StopShake();
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    void UpdateHealthUI()
    {
        if (healthText != null)
        {
            string hearts = "";
            for (int i = 0; i < currentHealth; i++) hearts += "X ";
            healthText.text = hearts;
        }
    }

    public void BuyWeapon()
    {
        if (isMerging) return;

        if (carStack.Count >= maxWeaponCount) return;
        int currentPrice = GetCurrentPrice();
        if (money >= currentPrice)
        {
            money -= currentPrice;
            SpawnWeapon();
        }
    }

    public void MergeWeapons()
    {
        if (isMerging || carStack.Count < 2) return;

        int mergeCost = GetNextMergeCost();

        if (HasMergeablePair())
        {
            if (money >= mergeCost)
            {
                money -= mergeCost;
                mergeCount++; // Sayacı artır
                StartCoroutine(MergeProcess());
            }
            else
            {
                Debug.Log("Merge için para yetersiz!");
            }
        }
        else
        {
            Debug.Log("Birleşecek uygun silah yok!");
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

                // 1. ÖNCE DİĞERLERİNİ KAYDIR (Beklemeden başlat)
                // Üstteki araçların kaymasını başlatıyoruz ki hepsi aynı anda hareket etsin.
                for (int k = i + 2; k < carStack.Count; k++)
                {
                    VehicleWeapon upperCar = carStack[k];
                    float targetY = ((k - 1) * carHeight) + (carHeight * 0.5f);
                    Vector3 targetLocalPos = new Vector3(0, targetY, 0);

                    StartCoroutine(upperCar.SmoothMove(targetLocalPos));
                }

                // 2. BİRLEŞEN SİLAHI GÖNDER VE HAREKETİN BİTMESİNİ BEKLE
                // WaitForSeconds(0.4f) YERİNE bunu kullanıyoruz.
                // Bu kod şu anlama gelir: "TopCar hedefe varana kadar bekle, varınca hemen devam et."
                yield return StartCoroutine(topCar.MoveAndMerge(bottomCar.transform.localPosition));

                // 3. HAREKET BİTTİĞİ GİBİ LEVEL ATLAT (Gecikme Yok)
                bottomCar.LevelUp();

                // Üstteki silahı temizle
                if (topCar != null && topCar.gameObject != null)
                {
                    Destroy(topCar.gameObject);
                }

                carStack.RemoveAt(i + 1);
                UpdatePositions();

                isMerging = false;
                UpdateUI();
                yield break;
            }
        }
    }

    void SpawnWeapon()
    {
        GameObject newCarObj = Instantiate(baseCarPrefab, transform);
        VehicleWeapon newCarScript = newCarObj.GetComponent<VehicleWeapon>();
        carStack.Add(newCarScript);
        UpdatePositions();
        UpdateUI();
    }

    void UpdatePositions()
    {
        for (int i = 0; i < carStack.Count; i++)
        {
            float targetY = (i * carHeight) + (carHeight * 0.5f);
            carStack[i].transform.localPosition = new Vector3(0, targetY, 0);
        }
    }

    void UpdateUI()
    {
        if (moneyText != null) moneyText.text = money.ToString() + " $";

        if (buyButtonComponent != null && buyButtonText != null)
        {
            if (carStack.Count >= maxWeaponCount)
            {
                buyButtonText.text = "MAX";
                buyButtonComponent.interactable = false;
            }
            else
            {
                int price = GetCurrentPrice();
                buyButtonText.text = "BUY " + price + "$";
                buyButtonComponent.interactable = (money >= price);
            }
        }

        if (mergeButtonComponent != null && mergeButtonText != null)
        {
            int mergeCost = GetNextMergeCost();
            bool canMerge = HasMergeablePair();

            if (canMerge)
            {
                mergeButtonText.text = "MERGE " + mergeCost + "$";
                mergeButtonComponent.interactable = (money >= mergeCost) && !isMerging;
            }
            else
            {
                mergeButtonText.text = "NO MERGE";
                mergeButtonComponent.interactable = false;
            }
        }
    }

    // --- FİYAT HESAPLAMALARI ---

    int GetCurrentPrice()
    {
        return basePrice * (carStack.Count + 1);
    }

    // --- YENİ DOĞRUSAL HESAPLAMA ---
    int GetNextMergeCost()
    {
        // 50 + (Sayaç * 2)
        return mergeBasePrice + (mergeCount * mergePriceIncreaseAmount);
    }

    bool HasMergeablePair()
    {
        if (carStack.Count < 2) return false;
        for (int i = 0; i < carStack.Count - 1; i++)
        {
            if (carStack[i].level == carStack[i + 1].level)
            {
                return true;
            }
        }
        return false;
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateUI();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}