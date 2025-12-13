using UnityEngine;
using System.Collections.Generic;
using TMPro; // UI ve Yazý iþlemleri için bu kütüphane ÞART!

public class VehicleStackManager : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject baseCarPrefab;
    public float carHeight = 1.2f;

    public List<VehicleWeapon> carStack = new List<VehicleWeapon>();

    [Header("Ekonomi")]
    public int money = 1000;
    public int basePrice = 50;

    [Header("UI Baðlantýlarý")]
    public TextMeshProUGUI moneyText; // Ekrandaki para yazýsý
    public TextMeshProUGUI buyButtonText; // Butonun üzerindeki fiyat yazýsý

    void Start()
    {
        SpawnCar();
        UpdateUI(); // Oyun baþlar baþlamaz yazýlarý güncelle
    }

    // Update fonksiyonundaki klavye kodlarýný sildim. Artýk buton kullanacaðýz.
    // Ýstersen test için B ve M tuþlarýný geri ekleyebilirsin.

    // --- BUTON ÝÇÝN SATIN ALMA ---
    public void BuyCar()
    {
        int currentPrice = GetCurrentPrice();

        if (money >= currentPrice)
        {
            money -= currentPrice;
            SpawnCar();
            UpdateUI(); // Para harcadýk, arayüzü güncelle
        }
        else
        {
            Debug.Log("Para Yetersiz!");
            // Ýstersen burada "Yetersiz Bakiye" animasyonu oynatabilirsin
        }
    }

    // --- BUTON ÝÇÝN MERGE ---
    public void MergeCars()
    {
        if (carStack.Count < 2) return;

        bool merged = false;

        for (int i = 0; i < carStack.Count - 1; i++)
        {
            VehicleWeapon bottomCar = carStack[i];
            VehicleWeapon topCar = carStack[i + 1];

            if (bottomCar.level == topCar.level)
            {
                bottomCar.LevelUp();
                Destroy(topCar.gameObject);
                carStack.RemoveAt(i + 1);

                merged = true;
                break; // Sadece bir tane birleþtir
            }
        }

        if (merged)
        {
            UpdatePositions();
            UpdateUI(); // Fiyat deðiþmiþ olabilir (Araç sayýsý azaldý), güncelle
        }
    }

    void SpawnCar()
    {
        GameObject newCarObj = Instantiate(baseCarPrefab, transform);
        newCarObj.transform.localPosition = Vector3.zero;

        VehicleWeapon newCarScript = newCarObj.GetComponent<VehicleWeapon>();
        carStack.Add(newCarScript);

        UpdatePositions();
        UpdateUI(); // Yeni araç geldi, fiyat arttý, güncelle
    }

    void UpdatePositions()
    {
        for (int i = 0; i < carStack.Count; i++)
        {
            Vector3 targetPos = new Vector3(0, i * carHeight, 0);
            carStack[i].transform.localPosition = targetPos;
        }
    }

    // --- YENÝ UI GÜNCELLEME FONKSÝYONU ---
    void UpdateUI()
    {
        // 1. Parayý güncelle
        if (moneyText != null)
            moneyText.text = money.ToString() + " $";

        // 2. Buton üzerindeki fiyatý güncelle
        if (buyButtonText != null)
        {
            int price = GetCurrentPrice();
            buyButtonText.text = "BUY\n" + price + " $";
        }
    }

    // Fiyat hesaplamayý ayrý fonksiyona aldým, her yerden çaðýrabilelim diye
    int GetCurrentPrice()
    {
        // Araç sayýsý arttýkça fiyat artsýn
        // Örn: 1. araç 50, 2. araç 100, 3. araç 150...
        return basePrice * (carStack.Count + 1);
    }

    // CarStackManager içine ekle:
    public void AddMoney(int amount)
    {
        money += amount;
        UpdateUI();
    }
}