using UnityEngine;
using TMPro; // Gerekirse kalsýn ama artýk Slot scripti üzerinden gideceðiz
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Header("YENÝ UI SLOTLARI (Inspector'dan Atanacak)")]
    // TextMeshProUGUI yerine artýk oluþturduðumuz scripti istiyoruz
    public UpgradeSlotUI slot_AtkPower;
    public UpgradeSlotUI slot_AtkSpeed;
    public UpgradeSlotUI slot_CritRate;
    public UpgradeSlotUI slot_CritDmg;
    public UpgradeSlotUI slot_MaxHealth;
    public UpgradeSlotUI slot_MaxStack;
    public UpgradeSlotUI slot_GoldGain;

    [Header("Upgrade Maliyetleri (Fiyat)")]
    public int baseCost_Atk = 40; public int costInc_Atk = 40;
    public int baseCost_Spd = 10; public int costInc_Spd = 5;
    public int baseCost_CritRate = 10; public int costInc_CritRate = 5;
    public int baseCost_CritDmg = 15; public int costInc_CritDmg = 15;
    public int baseCost_HP = 150; public int costInc_HP = 150;
    public int baseCost_Stack = 200; public int costInc_Stack = 250;
    public int baseCost_Gold = 15; public int costInc_Gold = 10;

    [Header("Upgrade Deðerleri (Level Baþýna Artýþ)")]
    public float incValue_Atk = 0.3f;
    public float incValue_Spd = 0.1f;
    public float incValue_CritRate = 1f;
    public float incValue_CritDmg = 0.2f;
    public int incValue_Gold = 1;

    void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        int currentGold = GetCurrentMoney();

        // 1. ATTACK POWER
        CalculateAndSetSlot(slot_AtkPower, "Upg_AtkPower", baseCost_Atk, costInc_Atk, currentGold,
            $"Increases damage by {incValue_Atk}");

        // 2. ATTACK SPEED
        CalculateAndSetSlot(slot_AtkSpeed, "Upg_AtkSpeed", baseCost_Spd, costInc_Spd, currentGold,
            $"Increases speed by {incValue_Spd}");

        // 3. CRIT RATE
        CalculateAndSetSlot(slot_CritRate, "Upg_CritRate", baseCost_CritRate, costInc_CritRate, currentGold,
            $"Increases crit chance by %{incValue_CritRate}");

        // 4. CRIT DMG
        CalculateAndSetSlot(slot_CritDmg, "Upg_CritDmg", baseCost_CritDmg, costInc_CritDmg, currentGold,
            $"Increases crit damage by x{incValue_CritDmg}");

        // 5. MAX HEALTH
        CalculateAndSetSlot(slot_MaxHealth, "Upg_MaxHealth", baseCost_HP, costInc_HP, currentGold,
            "Increases max health by 1 HP");

        // 6. MAX STACK (Özel Durum: Kilitli olabilir)
        UpdateMaxStackSlot(currentGold);

        // 7. GOLD GAIN
        CalculateAndSetSlot(slot_GoldGain, "Upg_GoldGain", baseCost_Gold, costInc_Gold, currentGold,
            $"Increases gold per stone by {incValue_Gold}");
    }

    // --- YENÝ YARDIMCI FONKSÝYON ---
    // Bu fonksiyon matematiði yapar ve sonucu Slot Scriptine gönderir
    void CalculateAndSetSlot(UpgradeSlotUI slot, string saveKey, int baseCost, int costInc, int playerGold, string description)
    {
        if (slot == null) return;

        int lvl = PlayerPrefs.GetInt(saveKey, 0);
        int cost = baseCost + (lvl * costInc);
        bool canAfford = (playerGold >= cost);

        // Slot scriptindeki UpdateSlot fonksiyonunu çaðýr
        slot.UpdateSlot(lvl, description, cost, canAfford);
    }

    // Stack Upgrade için özel kontrol (Limitler olduðu için ayrý tuttum)
    void UpdateMaxStackSlot(int currentGold)
    {
        if (slot_MaxStack == null || LevelGenerator.Instance == null) return;

        int lvl = PlayerPrefs.GetInt("Upg_MaxStack", 0);
        int cost = baseCost_Stack + (lvl * costInc_Stack);
        int purchasedStack = 3 + lvl; // Baþlangýç 3 + level

        int absoluteMax = LevelGenerator.Instance.absoluteMaxHeight;
        int currentLevelLimit = LevelGenerator.Instance.CurrentLevelMaxHeight;

        string desc = "Unlock +1 weapon slot";
        bool isInteractable = (currentGold >= cost);
        string priceText = cost.ToString();

        // 1. Mutlak Limite Ulaþýldý mý?
        if (purchasedStack >= absoluteMax)
        {
            slot_MaxStack.UpdateSlot(lvl, "Max Limit Reached", 0, false);
            if (slot_MaxStack.priceText != null) slot_MaxStack.priceText.text = "MAX";
            return;
        }

        // 2. Level Limidine Takýldý mý?
        if (purchasedStack >= currentLevelLimit)
        {
            // Kaçýncý levelde açýlacaðýný hesapla
            int startH = LevelGenerator.Instance.startMaxHeight;
            int incX = LevelGenerator.Instance.increaseEveryXLevel;
            int neededHeight = purchasedStack + 1;
            int targetLevel = ((neededHeight - startH) * incX) + 1;

            if (LevelManager.Instance != null && targetLevel <= LevelManager.Instance.currentLevel)
                targetLevel = LevelManager.Instance.currentLevel + 1;

            desc = $"LOCKED! Unlocks at Level {targetLevel}";
            isInteractable = false; // Kilitli olduðu için alýnamaz
        }

        // Slotu Güncelle
        slot_MaxStack.UpdateSlot(lvl, desc, cost, isInteractable);
    }

    // --- SATIN ALMA FONKSÝYONLARI (Aynen Kalýyor) ---
    public void BuyUpgrade(string saveKey, int baseCost, int costInc)
    {
        int currentGold = GetCurrentMoney();
        int currentLevel = PlayerPrefs.GetInt(saveKey, 0);
        int cost = baseCost + (currentLevel * costInc);

        if (currentGold >= cost)
        {
            currentGold -= cost;
            SaveMoney(currentGold);
            PlayerPrefs.SetInt(saveKey, currentLevel + 1);
            PlayerPrefs.Save();
            UpdateUI(); // UI'ý yenile
        }
    }

    // Butonlarýn OnClick eventine baðlanacak fonksiyonlar
    public void BuyUpgrade_MaxHealth() { BuyUpgrade("Upg_MaxHealth", baseCost_HP, costInc_HP); if (VehicleStackManager.Instance != null) VehicleStackManager.Instance.OnHealthUpgradeBought(); }
    public void BuyUpgrade_GoldGain() { BuyUpgrade("Upg_GoldGain", baseCost_Gold, costInc_Gold); }
    public void BuyUpgrade_AttackPower() { BuyUpgrade("Upg_AtkPower", baseCost_Atk, costInc_Atk); }
    public void BuyUpgrade_AttackSpeed() { BuyUpgrade("Upg_AtkSpeed", baseCost_Spd, costInc_Spd); }
    public void BuyUpgrade_CritRate() { BuyUpgrade("Upg_CritRate", baseCost_CritRate, costInc_CritRate); }
    public void BuyUpgrade_CritDmg() { BuyUpgrade("Upg_CritDmg", baseCost_CritDmg, costInc_CritDmg); }

    public void BuyUpgrade_MaxStack()
    {
        if (LevelGenerator.Instance != null)
        {
            int purchasedStack = 3 + PlayerPrefs.GetInt("Upg_MaxStack", 0);
            if (purchasedStack >= LevelGenerator.Instance.CurrentLevelMaxHeight) return; // Kilitliyse alma
        }
        BuyUpgrade("Upg_MaxStack", baseCost_Stack, costInc_Stack);
        if (VehicleStackManager.Instance != null) VehicleStackManager.Instance.UpdateUI();
    }

    int GetCurrentMoney() { if (VehicleStackManager.Instance != null) return VehicleStackManager.Instance.money; return PlayerPrefs.GetInt("TotalGold", 0); }
    void SaveMoney(int amount) { if (VehicleStackManager.Instance != null) { VehicleStackManager.Instance.money = amount; VehicleStackManager.Instance.UpdateUI(); } PlayerPrefs.SetInt("TotalGold", amount); PlayerPrefs.Save(); }
}