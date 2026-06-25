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
        public float value; // Bu yeni perk için buraya ne yazdığının önemi yok (0 kalabilir)
    }

    public enum PerkType { DamageBoost, FireRateBoost, CritChanceBoost, GoldBoost, UpgradeLowest, RandomFreeUpgrade }

    [Header("Ayarlar")]
    public GameObject perkPanel;
    public PerkCardUI[] cardSlots; // Script isminin projendeki adla (PerkCardUI) aynı olduğuna emin ol.
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

        // --- 1. GLOBAL STAT PERKLERİ (Tüm silahları anında etkiler) ---
        if (perk.type == PerkType.DamageBoost)
        {
            VehicleWeapon.globalPerkDamageMultiplier += perk.value;
            Debug.Log($" [PERK UYGULANDI] Hasar Artırıldı! Eklenen: +{perk.value} | Yeni Hasar Çarpanı: {VehicleWeapon.globalPerkDamageMultiplier}x");
        }
        else if (perk.type == PerkType.FireRateBoost)
        {
            VehicleWeapon.globalPerkFireRateMultiplier += perk.value;
            Debug.Log($" [PERK UYGULANDI] Atış Hızı Artırıldı! Eklenen: +{perk.value} | Yeni Hız Çarpanı: {VehicleWeapon.globalPerkFireRateMultiplier}x");
        }
        else if (perk.type == PerkType.CritChanceBoost)
        {
            VehicleWeapon.globalPerkCritChanceAdd += perk.value;
            Debug.Log($" [PERK UYGULANDI] Kritik Şansı Artırıldı! Eklenen: +%{perk.value} | Toplam Ekstra Şans: %{VehicleWeapon.globalPerkCritChanceAdd}");
        }
        // --- 2. ALTIN VE SİLAH LEVELİ PERKLERİ ---
        else if (stackManager != null)
        {
            if (perk.type == PerkType.GoldBoost)
            {
                stackManager.tempGoldMultiplier += perk.value;
            }
            else if (perk.type == PerkType.UpgradeLowest)
            {
                stackManager.UpgradeLowestLevelWeapon();
            }
        }

                // --- 3. BEDAVA UPGRADE PERKİ (AD TRIGGER) ---
        if (perk.type == PerkType.RandomFreeUpgrade)
        {
            // Eğer bu perk seçilirse reklam yöneticisini çağırıyoruz
            if (CrazyGamesManager.Instance != null)
            {
                CrazyGamesManager.Instance.RewardedAdShow("RandomFreeUpgrade");
            }
            // Reklam açılacağı için paneli şimdi kapatmıyoruz ve oyunu devam ettirmiyoruz.
            // Ödül izlendikten sonra CrazyGamesManager zaten GrantRandomFreeUpgrade'i çağırıp ekranı kapatacak.
            return; 
        }
// Paneli Kapat ve Oyunu Devam Ettir
        perkPanel.SetActive(false);
        Time.timeScale = 1f;
        isPerkActive = false;
    }
}
