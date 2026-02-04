using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Header("UI SLOTLARI")]
    public UpgradeSlotUI slot_AtkPower;
    public UpgradeSlotUI slot_AtkSpeed;
    public UpgradeSlotUI slot_CritRate;
    public UpgradeSlotUI slot_CritDmg;
    public UpgradeSlotUI slot_MaxHealth;
    public UpgradeSlotUI slot_MaxStack;
    public UpgradeSlotUI slot_GoldGain;

    [Header("Upgrade Maliyetleri (Fiyat)")]
    public int baseCost_Atk = 40; public int costInc_Atk = 60;
    public int baseCost_Spd = 15; public int costInc_Spd = 30;
    public int baseCost_CritRate = 20; public int costInc_CritRate = 10;
    public int baseCost_CritDmg = 35; public int costInc_CritDmg = 20;
    public int baseCost_HP = 150; public int costInc_HP = 150;
    public int baseCost_Stack = 200; public int costInc_Stack = 250;
    public int baseCost_Gold = 30; public int costInc_Gold = 15;

    [Header("Upgrade Deðerleri")]
    public float incValue_Atk = 1f;
    public float incValue_Spd = 0.2f;
    public float incValue_CritRate = 1f;
    public float incValue_CritDmg = 0.2f;
    public int incValue_Gold = 2;

    // --- LÝMÝTLER ---
    private int absoluteMaxLimit_Stack = 8; // Silah slotu limiti
    private int maxLevel_CritRate = 30;     // Crit Rate limiti (YENÝ)

    private string[] randomUpgradeCandidates = new string[]
    {
        "Upg_AtkPower",
        "Upg_AtkSpeed",
        "Upg_CritRate",
        "Upg_CritDmg",
        "Upg_GoldGain"
    };

    void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        int currentGold = GetCurrentMoney();

        // 1. ATTACK POWER (Sýnýr yok)
        CalculateAndSetSlot(slot_AtkPower, "Upg_AtkPower", baseCost_Atk, costInc_Atk, currentGold,
            $"Increases attack damage \n+{incValue_Atk}");

        // 2. ATTACK SPEED (Sýnýr yok)
        CalculateAndSetSlot(slot_AtkSpeed, "Upg_AtkSpeed", baseCost_Spd, costInc_Spd, currentGold,
            $"Increases attack speed \n+{incValue_Spd}");

        // 3. CRIT RATE (Max Level 30 Sýnýrý Var!)
        // Buraya 'maxLevel_CritRate' deðiþkenini gönderiyoruz.
        CalculateAndSetSlot(slot_CritRate, "Upg_CritRate", baseCost_CritRate, costInc_CritRate, currentGold,
            $"Increases critical chance \n+%{incValue_CritRate}", maxLevel_CritRate);

        // 4. CRIT DMG (Sýnýr yok)
        CalculateAndSetSlot(slot_CritDmg, "Upg_CritDmg", baseCost_CritDmg, costInc_CritDmg, currentGold,
            $"Increases critical damage \n+X{incValue_CritDmg}");

        // 5. MAX HEALTH (Sýnýr yok)
        CalculateAndSetSlot(slot_MaxHealth, "Upg_MaxHealth", baseCost_HP, costInc_HP, currentGold,
            "Increases max health \n+1 HP");

        // 6. MAX STACK (Özel fonksiyonu var)
        UpdateMaxStackSlot(currentGold);

        // 7. GOLD GAIN (Sýnýr yok)
        CalculateAndSetSlot(slot_GoldGain, "Upg_GoldGain", baseCost_Gold, costInc_Gold, currentGold,
            $"Increases won gold per stone \n+{incValue_Gold}");
    }

    // --- GÜNCELLENEN FONKSÝYON: MaxLevel Parametresi Eklendi ---
    // varsayýlan deðer (int.MaxValue) verdik ki diðerlerinde deðiþiklik yapmamýza gerek kalmasýn.
    void CalculateAndSetSlot(UpgradeSlotUI slot, string saveKey, int baseCost, int costInc, int playerGold, string description, int maxLevel = int.MaxValue)
    {
        if (slot == null) return;

        int lvl = PlayerPrefs.GetInt(saveKey, 0);

        // --- MAX LEVEL KONTROLÜ ---
        if (lvl >= maxLevel)
        {
            slot.UpdateSlot(lvl, "Max Level Reached", 0, false);
            if (slot.priceText != null) slot.priceText.text = "MAX";
            return;
        }
        // --------------------------

        int cost = baseCost + (lvl * costInc);
        bool canAfford = (playerGold >= cost);

        slot.UpdateSlot(lvl, description, cost, canAfford);
    }

    void UpdateMaxStackSlot(int currentGold)
    {
        if (slot_MaxStack == null) return;

        int lvl = PlayerPrefs.GetInt("Upg_MaxStack", 0);
        int cost = baseCost_Stack + (lvl * costInc_Stack);
        int currentCapacity = 4 + lvl;

        string desc = "Unlock +1 weapon slot";
        bool isInteractable = (currentGold >= cost);

        if (currentCapacity >= absoluteMaxLimit_Stack)
        {
            slot_MaxStack.UpdateSlot(lvl, "Max Limit Reached", 0, false);
            if (slot_MaxStack.priceText != null) slot_MaxStack.priceText.text = "MAX";
            return;
        }

        slot_MaxStack.UpdateSlot(lvl, desc, cost, isInteractable);
    }

    // --- GÜNCELLENEN SATIN ALMA: MaxLevel Parametresi Eklendi ---
    public void BuyUpgrade(string saveKey, int baseCost, int costInc, int maxLevel = int.MaxValue)
    {
        int currentLevel = PlayerPrefs.GetInt(saveKey, 0);

        // Eðer zaten max seviyedeysek alma
        if (currentLevel >= maxLevel) return;

        int currentGold = GetCurrentMoney();
        int cost = baseCost + (currentLevel * costInc);

        if (currentGold >= cost)
        {
            currentGold -= cost;
            SaveMoney(currentGold);
            PlayerPrefs.SetInt(saveKey, currentLevel + 1);
            PlayerPrefs.Save();
            UpdateUI();

            if (VehicleStackManager.Instance != null)
            {
                VehicleStackManager.Instance.PlayUpgradeEffect();
            }
        }


    }

    public void ApplyRandomFreeUpgrade()
    {
        // 1. Geçerli (Fullenmemiþ) Upgradeleri Bul
        List<string> validKeys = new List<string>();

        foreach (string key in randomUpgradeCandidates)
        {
            int currentLvl = PlayerPrefs.GetInt(key, 0);

            // Eðer CritRate ise ve 30 olduysa listeye ekleme
            if (key == "Upg_CritRate" && currentLvl >= maxLevel_CritRate)
                continue;

            // Diðerlerinde þu an sýnýr yok, listeye ekle
            validKeys.Add(key);
        }

        // 2. Eðer yükseltilecek bir þey kalmadýysa (Nadir durum)
        if (validKeys.Count == 0)
        {
            Debug.Log("Tüm rastgele upgradeler zaten MAX seviyede!");
            return;
        }

        // 3. Rastgele Seç
        string selectedKey = validKeys[Random.Range(0, validKeys.Count)];

        // 4. ÜCRETSÝZ YÜKSELT (Para düþmeden level arttýr)
        int lvl = PlayerPrefs.GetInt(selectedKey, 0);
        PlayerPrefs.SetInt(selectedKey, lvl + 1);
        PlayerPrefs.Save();

        // 5. Görsel Efekt ve Güncelleme
        UpdateUI();
        if (VehicleStackManager.Instance != null)
        {
            VehicleStackManager.Instance.PlayUpgradeEffect();
            PerkManager.Instance.isPerkActive = false; // Perk ekranýný kapat
            PerkManager.Instance.perkPanel.SetActive(false);
        }

        Debug.Log($"REKLAM ÖDÜLÜ: {selectedKey} ücretsiz yükseltildi!");
    }
    // ----------------------------------------

    // --- BUTON BAÐLANTILARI ---

    // CRIT RATE ÝÇÝN LÝMÝTÝ BURADAN GÖNDERÝYORUZ (maxLevel_CritRate)
    public void BuyUpgrade_CritRate() { BuyUpgrade("Upg_CritRate", baseCost_CritRate, costInc_CritRate, maxLevel_CritRate); }

    // Diðerleri sýnýrsýz (Varsayýlan int.MaxValue kullanýrlar)
    public void BuyUpgrade_MaxHealth() { BuyUpgrade("Upg_MaxHealth", baseCost_HP, costInc_HP); if (VehicleStackManager.Instance != null) VehicleStackManager.Instance.OnHealthUpgradeBought(); }
    public void BuyUpgrade_GoldGain() { BuyUpgrade("Upg_GoldGain", baseCost_Gold, costInc_Gold); }
    public void BuyUpgrade_AttackPower() { BuyUpgrade("Upg_AtkPower", baseCost_Atk, costInc_Atk); }
    public void BuyUpgrade_AttackSpeed() { BuyUpgrade("Upg_AtkSpeed", baseCost_Spd, costInc_Spd); }
    public void BuyUpgrade_CritDmg() { BuyUpgrade("Upg_CritDmg", baseCost_CritDmg, costInc_CritDmg); }

    public void BuyUpgrade_MaxStack()
    {
        int currentCapacity = 4 + PlayerPrefs.GetInt("Upg_MaxStack", 0);
        if (currentCapacity >= absoluteMaxLimit_Stack) return;

        BuyUpgrade("Upg_MaxStack", baseCost_Stack, costInc_Stack);
        if (VehicleStackManager.Instance != null) VehicleStackManager.Instance.UpdateUI();
    }

    int GetCurrentMoney() { if (VehicleStackManager.Instance != null) return VehicleStackManager.Instance.money; return PlayerPrefs.GetInt("TotalGold", 0); }
    void SaveMoney(int amount) { if (VehicleStackManager.Instance != null) { VehicleStackManager.Instance.money = amount; VehicleStackManager.Instance.UpdateUI(); } PlayerPrefs.SetInt("TotalGold", amount); PlayerPrefs.Save(); }
}