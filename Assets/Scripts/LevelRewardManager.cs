using UnityEngine;
using TMPro;
using UnityEngine.UI; // YENÝ EKLENDÝ: Buton kontrolü için gerekli

public class LevelRewardManager : MonoBehaviour
{
    [Header("UI Elemanlarý")]
    public TextMeshProUGUI goldText;

    [Header("2x Buton Ayarlarý (YENÝ)")]
    public Button doubleRewardButton;  // Týklanmaz yapmak istediðimiz buton
    public GameObject adIconsGroup;    // Gizlemek istediðimiz "2x" ve "Kamera" ikonlarý

    [Header("Ayarlar")]
    public int baseGold = 50;
    public int increaseAmount = 50;
    public int penaltyPerHealth = 10;

    private int lastCalculatedReward = 0;

    private void Start()
    {
        // Yeni level yüklendiðinde, ölümsüz CrazyGamesManager'a "Yeni yönetici benim" diyoruz.
        if (CrazyGamesManager.Instance != null)
        {
            CrazyGamesManager.Instance.levelRewardManager = this;
            Debug.Log("LevelRewardManager baþarýyla CrazyGamesManager'a baðlandý!");
        }
        else
        {
            Debug.LogWarning("CrazyGamesManager sahnede bulunamadý!");
        }
    }
    public void ShowReward(int level, int currentHealth, int maxHealth)
    {
        // ... (Burasý ayný kalacak) ...
        int levelMultiplier = level / 5;
        int calculatedBaseGold = baseGold + (levelMultiplier * increaseAmount);
        int missingHealth = maxHealth - currentHealth;
        int penalty = missingHealth * penaltyPerHealth;

        lastCalculatedReward = Mathf.Max(0, calculatedBaseGold - penalty);

        goldText.text = lastCalculatedReward.ToString();
        AddGoldToWallet(lastCalculatedReward);

        // Her level sonunda butonu tekrar aktif hale getiriyoruz (Resetliyoruz)
        if (doubleRewardButton != null) doubleRewardButton.interactable = true;
        if (adIconsGroup != null) adIconsGroup.SetActive(true);
    }

    public void ActivateDoubleReward()
    {
        Debug.Log("Ödül ikiye katlanýyor...");

        AddGoldToWallet(lastCalculatedReward);
        goldText.text = (lastCalculatedReward * 2).ToString();

        // --- YENÝ KISIM BAÞLANGIÇ ---

        // 1. Ýkonlarý Gizle
        if (adIconsGroup != null)
        {
            adIconsGroup.SetActive(false);
        }

        // 2. Butonu Týklanmaz Yap (Griye döner ve basýlamaz)
        if (doubleRewardButton != null)
        {
            doubleRewardButton.interactable = false;
        }

        // --- YENÝ KISIM BÝTÝÞ ---
    }

    private void AddGoldToWallet(int amount)
    {
        int currentMoney = PlayerPrefs.GetInt("TotalGold", 0);
        PlayerPrefs.SetInt("TotalGold", currentMoney + amount);
        PlayerPrefs.Save();
    }

    // ... Diðer fonksiyonlarýn (AdRevive, AdFreeGold vb.) aynen kalacak ...
    public void AdRevive() { VehicleStackManager.Instance.Revive(); }
    public void AdFreeGold() { VehicleStackManager.Instance.AddMoney(300); RewardButtons.Instance.HideCurrent(); }
    public void AdActivateSlow() { GameManager.Instance.SlowGame(15f); RewardButtons.Instance.HideCurrent(); }
    public void AdFreeHealth() { VehicleStackManager.Instance.AddHealth(); RewardButtons.Instance.HideCurrent(); }
    public void RandomFreeUpgrade() { UpgradeManager.Instance.ApplyRandomFreeUpgrade(); RewardButtons.Instance.HideCurrent(); }
}