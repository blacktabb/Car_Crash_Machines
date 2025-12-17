using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class VehicleStackManager : MonoBehaviour
{
    [Header("Oyun Durumu")]
    public int maxHealth = 3;
    private int currentHealth;
    public bool isGameOver = false;

    [Header("Ayarlar")]
    public GameObject baseCarPrefab;
    public float carHeight = 1.0f;
    public int maxWeaponCount = 6;

    public List<VehicleWeapon> carStack = new List<VehicleWeapon>();

    [Header("Ekonomi & UI")]
    public int money = 1000;
    public int basePrice = 50;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI buyButtonText;
    public Button buyButtonComponent;

    [Header("UI")]
    public GameObject gameOverPanel;

    [Header("Hasar Ayarı")]
    public float damageCooldown = 0.5f; // Yarım saniye ölümsüzlük
    private float nextDamageTime = 0f;  // Bir sonraki hasar ne zaman alınabilir?

    [Header("Efektler")]
    public GameObject hitParticlePrefab; // Duvara çarpma efekti
    public CameraShake cameraShake; // Kamera titretme scripti
    public float shakeDuration = 0.2f; // Ne kadar sürsün?
    public float shakeMagnitude = 0.1f; // Ne kadar şiddetli olsun?

    // --- CAN GÖSTERGESİ İÇİN ---
    public TextMeshProUGUI healthText; // Canı yazmak için (Kalp emojisiyle)

    void Start()
    {
        currentHealth = maxHealth; // Canı fulle
        UpdateHealthUI();
        SpawnCar();
    }

    // --- ÇARPIŞMA ALGILAMA (TRIGGER) ---
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isGameOver) return;

        // --- DÜZELTME BURADA ---
        // Eğer şu an "Ölümsüzlük" süresindeysek, çarpışmayı yok say.
        if (Time.time < nextDamageTime)
        {
            return;
        }

        if (other.CompareTag("Stone"))
        {
            // Bir sonraki hasar için zamanı ileriye atıyoruz
            nextDamageTime = Time.time + damageCooldown;

            TakeDamage(other.gameObject);
        }
    }

    void TakeDamage(GameObject stoneObj)
    {
        // ... (currentHealth-- ve UpdateHealthUI() kodları burada kalacak) ...
        currentHealth--;
        UpdateHealthUI();

        // --- YENİ EKLENEN EFEKTLER ---
        // 1. Çarpma noktasında parçacık efekti oluştur
        if (hitParticlePrefab != null)
        {
            // Taşın tam olduğu yerde efekti patlat
            Instantiate(hitParticlePrefab, stoneObj.transform.position, Quaternion.identity);
        }

        // 2. Ekranı Titret
        if (cameraShake != null)
        {
            cameraShake.TriggerShake(shakeDuration, shakeMagnitude);
        }
        // -----------------------------

        float hitX = stoneObj.transform.position.x;
        Destroy(stoneObj);
        ClearColumn(hitX);

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    // Taşa çarptığımızda o hizadaki tüm taşları yok eden fonksiyon
    // Eski ClearColumn yerine bunu yapıştır:
    void ClearColumn(float collisionX)
    {
        // Tarama başlangıç noktası (Çarpışmanın olduğu X, bizim Y)
        Vector2 startPoint = new Vector2(collisionX, transform.position.y);

        // --- DEĞİŞİKLİK BURADA: BoxCastAll ---
        // Raycast (Çizgi) yerine BoxCast (Kutu) atıyoruz.
        // Size: Genişliği 1.5f (Hafif geniş olsun ki sağ solu da kapsasın), Yüksekliği 0.1f (Önemsiz, yönle uzayacak)
        // Angle: 0
        // Direction: Yukarı (Vector2.up)
        // Distance: 20f (Yukarı kadar tara)

        RaycastHit2D[] hits = Physics2D.BoxCastAll(startPoint, new Vector2(1.5f, 0.1f), 0f, Vector2.up, 20f);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.CompareTag("Stone"))
            {
                // Efekt eklenebilir (Instantiate particle...)
                Destroy(hit.collider.gameObject);
            }
        }
    }

    void GameOver()
    {
        isGameOver = true;
        Debug.Log("OYUN BİTTİ!");

        // 1. TİTREMEYİ DURDUR (Kamera yamuk kalmasın)
        if (cameraShake != null)
        {
            cameraShake.StopShake();
        }

        // 2. PANELİ AÇ
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // 3. OYUNU DURDUR
        Time.timeScale = 0f;

        // (Eski text kodunu silebilirsin çünkü artık panelimiz var)
        // if (healthText != null) healthText.text = "GAME OVER"; 
    }

    void UpdateHealthUI()
    {
        if (healthText != null)
        {
            // Can sayısı kadar Kalp koyalım
            string hearts = "";
            for (int i = 0; i < currentHealth; i++) hearts += "X ";
            healthText.text = hearts;
        }
    }

    // ... (BuyCar, MergeCars, SpawnCar, UpdatePositions, UpdateUI aynen kalacak) ...
    // Sadece mevcut fonksiyonların altına yapıştırabilirsin veya class'ı koruyarak ekle.

    // --- AŞAĞIDAKİLER ESKİ KODLARIN AYNI KALACAK ---
    public void BuyCar()
    {
        if (carStack.Count >= maxWeaponCount) return;
        int currentPrice = GetCurrentPrice();
        if (money >= currentPrice)
        {
            money -= currentPrice;
            SpawnCar();
        }
    }

    public void MergeCars()
    {
        if (carStack.Count < 2) return;
        for (int i = 0; i < carStack.Count - 1; i++)
        {
            VehicleWeapon bottomCar = carStack[i];
            VehicleWeapon topCar = carStack[i + 1];
            if (bottomCar.level == topCar.level)
            {
                bottomCar.LevelUp();
                topCar.DestroyWithAnimation();
                carStack.RemoveAt(i + 1);
                UpdatePositions();
                UpdateUI();
                return;
            }
        }
    }

    void SpawnCar()
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
                buyButtonComponent.interactable = true;
            }
        }
    }

    int GetCurrentPrice()
    {
        return basePrice * (carStack.Count + 1);
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateUI();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Zamanı tekrar akıt
        SceneManager.LoadScene("MainMenu");
    }
}