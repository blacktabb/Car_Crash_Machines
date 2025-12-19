using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    [Header("UI Referanslarý")]
    public TextMeshProUGUI totalGoldText;
    public GameObject shopPanel;

    [Header("Upgrade Buton Textleri")]
    public TextMeshProUGUI atkPowerText;
    public TextMeshProUGUI atkSpeedText;
    public TextMeshProUGUI critRateText;
    public TextMeshProUGUI critDmgText;
    public TextMeshProUGUI maxHealthText;

    // --- AYARLAR (Fiyatlar ve Artýþ Oranlarý) ---
    private int baseCost_Atk = 100; private int costInc_Atk = 50;
    private int baseCost_Spd = 150; private int costInc_Spd = 75;
    private int baseCost_CritRate = 200; private int costInc_CritRate = 100;
    private int baseCost_CritDmg = 200; private int costInc_CritDmg = 100;
    private int baseCost_HP = 300; private int costInc_HP = 150;

    void Start()
    {
        UpdateUI();
        shopPanel.SetActive(false);
    }

    public void BuyUpgrade_AttackPower() { BuyUpgrade("Upg_AtkPower", baseCost_Atk, costInc_Atk); }
    public void BuyUpgrade_AttackSpeed() { BuyUpgrade("Upg_AtkSpeed", baseCost_Spd, costInc_Spd); }
    public void BuyUpgrade_CritRate() { BuyUpgrade("Upg_CritRate", baseCost_CritRate, costInc_CritRate); }
    public void BuyUpgrade_CritDmg() { BuyUpgrade("Upg_CritDmg", baseCost_CritDmg, costInc_CritDmg); }
    public void BuyUpgrade_MaxHealth() { BuyUpgrade("Upg_MaxHealth", baseCost_HP, costInc_HP); }

    void BuyUpgrade(string saveKey, int baseCost, int costInc)
    {
        int currentGold = PlayerPrefs.GetInt("TotalGold", 0);
        int currentLevel = PlayerPrefs.GetInt(saveKey, 0);
        int cost = baseCost + (currentLevel * costInc);

        if (currentGold >= cost)
        {
            currentGold -= cost;
            PlayerPrefs.SetInt("TotalGold", currentGold);
            PlayerPrefs.SetInt(saveKey, currentLevel + 1);
            PlayerPrefs.Save();
            UpdateUI();
        }
        else
        {
            Debug.Log("Para yetersiz!");
        }
    }

    // --- BURASI GÜNCELLENDÝ: ARTIK DEÐERLERÝ DE HESAPLIYORUZ ---
    void UpdateUI()
    {
        int gold = PlayerPrefs.GetInt("TotalGold", 0);
        totalGoldText.text = gold.ToString() + " GOLD";

        // 1. ATTACK POWER (Her level +5 Hasar)
        int lvl_Atk = PlayerPrefs.GetInt("Upg_AtkPower", 0);
        string val_Atk = "+" + (lvl_Atk * 5) + " DMG";
        UpdateBtnText(atkPowerText, "Upg_AtkPower", baseCost_Atk, costInc_Atk, "ATK POWER", val_Atk);

        // 2. ATTACK SPEED (Her level %2 Hýzlanma)
        int lvl_Spd = PlayerPrefs.GetInt("Upg_AtkSpeed", 0);
        string val_Spd = "+" + (lvl_Spd * 2) + "% SPEED";
        UpdateBtnText(atkSpeedText, "Upg_AtkSpeed", baseCost_Spd, costInc_Spd, "ATK SPEED", val_Spd);

        // 3. CRIT RATE (Her level %2 Þans)
        int lvl_Crit = PlayerPrefs.GetInt("Upg_CritRate", 0);
        // Taban %5 þansýmýz vardý, üzerine ekliyoruz
        string val_Crit = (5 + (lvl_Crit * 2)) + "% CHANCE";
        UpdateBtnText(critRateText, "Upg_CritRate", baseCost_CritRate, costInc_CritRate, "CRIT RATE", val_Crit);

        // 4. CRIT DAMAGE (Taban 1.5x, Her level +0.1x)
        int lvl_CDmg = PlayerPrefs.GetInt("Upg_CritDmg", 0);
        string val_CDmg = (1.5f + (lvl_CDmg * 0.1f)).ToString("F1") + "x DMG"; // F1: Virgülden sonra tek hane (1.6x gibi)
        UpdateBtnText(critDmgText, "Upg_CritDmg", baseCost_CritDmg, costInc_CritDmg, "CRIT DMG", val_CDmg);

        // 5. MAX HEALTH (Taban 3 Can, Her level +1 Can)
        int lvl_HP = PlayerPrefs.GetInt("Upg_MaxHealth", 0);
        string val_HP = (3 + lvl_HP) + " HP";
        UpdateBtnText(maxHealthText, "Upg_MaxHealth", baseCost_HP, costInc_HP, "MAX HEALTH", val_HP);
    }

    // --- YENÝ FORMAT ---
    // Baþlýk, Ýstatistik, Level ve Fiyatý alt alta yazar
    void UpdateBtnText(TextMeshProUGUI txt, string key, int baseCost, int inc, string title, string currentStat)
    {
        if (txt == null) return;

        int lvl = PlayerPrefs.GetInt(key, 0);
        int cost = baseCost + (lvl * inc);

        // ÖRNEK GÖRÜNÜM:
        // ATK POWER (+20 DMG)
        // Lvl 4
        // Price: 300
        txt.text = $"{title} <color=yellow>({currentStat})</color>\nLvl {lvl}\nPrice: {cost}";
    }

    public void ToggleShop()
    {
        shopPanel.SetActive(!shopPanel.activeSelf);
        if (shopPanel.activeSelf) UpdateUI();
    }

    void Update() { if (Input.GetKeyDown(KeyCode.K)) { PlayerPrefs.SetInt("TotalGold", 10000); UpdateUI(); } }
}