using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PerkManager : MonoBehaviour
{
    public static PerkManager Instance;

    [System.Serializable]
    public class PerkData
    {
        public string title;
        public string description;
        public Sprite icon;
        public PerkType type;
        public float value; // Bu yeni perk için buraya ne yazdýðýnýn önemi yok (0 kalabilir)
    }

    // 1. DEÐÝÞÝKLÝK: Yeni tipi buraya ekledik (UpgradeLowest)
    public enum PerkType { DamageBoost, FireRateBoost, CritChanceBoost, GoldBoost, UpgradeLowest, RandomFreeUpgrade }

    [Header("Ayarlar")]
    public GameObject perkPanel;
    public PerkCardUI[] cardSlots;
    public List<PerkData> availablePerks;

    [Header("Durum")]
    public bool isPerkActive = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        perkPanel.SetActive(false);
    }

    public void ShowPerkSelection()
    {
        if (availablePerks.Count < 3) return;

        isPerkActive = true;
        Time.timeScale = 0f;
        perkPanel.SetActive(true);

        List<PerkData> selectedPerks = new List<PerkData>();
        List<int> usedIndexes = new List<int>();

        for (int i = 0; i < 3; i++)
        {
            int rnd;
            do { rnd = Random.Range(0, availablePerks.Count); }
            while (usedIndexes.Contains(rnd));

            usedIndexes.Add(rnd);
            selectedPerks.Add(availablePerks[rnd]);
        }

        for (int i = 0; i < 3; i++)
        {
            cardSlots[i].Setup(selectedPerks[i], this);
        }
    }

    public void ApplyPerk(PerkData perk)
    {
        VehicleStackManager stackManager = Object.FindFirstObjectByType<VehicleStackManager>();
        UpgradeManager upgradeManager = Object.FindFirstObjectByType<UpgradeManager>();

        if (stackManager != null)
        {
            // A) EÐER ALTIN BOOST ÝSE:
            if (perk.type == PerkType.GoldBoost)
            {
                stackManager.tempGoldMultiplier += perk.value;
            }
            // 2. DEÐÝÞÝKLÝK: EÐER "EN DÜÞÜÐÜ YÜKSELT" ÝSE:
            else if (perk.type == PerkType.UpgradeLowest)
            {
                // VehicleStackManager'daki fonksiyonu tetikliyoruz
                stackManager.UpgradeLowestLevelWeapon();
            }
            // C) EÐER STAT (HASAR/HIZ) BOOST ÝSE:
            else 
            {
                foreach (VehicleWeapon weapon in stackManager.carStack)
                {
                    ApplyToWeapon(weapon, perk);
                }
            }
        }

        // Paneli Kapat ve Devam Et
        perkPanel.SetActive(false);
        Time.timeScale = 1f;
        isPerkActive = false;
    }

    public void ApplyToWeapon(VehicleWeapon weapon, PerkData perk)
    {
        switch (perk.type)
        {
            case PerkType.DamageBoost:
                weapon.tempDamageMultiplier += perk.value;
                break;
            case PerkType.FireRateBoost:
                weapon.tempFireRateDivider += perk.value;
                break;
            case PerkType.CritChanceBoost:
                weapon.tempCritChanceAdd += perk.value;
                break;
                // UpgradeLowest buraya girmeyecek çünkü yukarýda else if ile ayýrdýk.
        }
    }
}