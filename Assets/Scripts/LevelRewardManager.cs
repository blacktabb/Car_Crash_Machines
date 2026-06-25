using UnityEngine;
using TMPro;
using UnityEngine.UI; // YEN EKLEND: Buton kontrol iin gerekli

public class LevelRewardManager : MonoBehaviour
{
    [Header("UI Elemanlar")]
    public TextMeshProUGUI goldText;

    [Header("2x Buton Ayarlar (YEN)")]
    public Button doubleRewardButton;  // Tklanmaz yapmak istediimiz buton
    public GameObject adIconsGroup;    // Gizlemek istediimiz "2x" ve "Kamera" ikonlar

    [Header("Ayarlar")]
    public int baseGold = 50;
    public int increaseAmount = 50;
    public int penaltyPerHealth = 10;

    private int lastCalculatedReward = 0;

    private void Start()
    {
        // Yeni level yklendiinde, lmsz CrazyGamesManager'a "Yeni ynetici benim" diyoruz.
                if (CrazyGamesManager.Instance != null)
        {
            CrazyGamesManager.Instance.levelRewardManager = this;
            Debug.Log("LevelRewardManager ba...");
        }
        else
        {
            Debug.LogWarning("CrazyGamesManager sahnede bulunamad...");
        }
    }
    public void ShowReward(int level, int currentHealth, int maxHealth)
    {
        // ... (Buras ayn kalacak) ...
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

        // --- AD TRIGGER METHODS (Butonlardan arlan) ---
    public void ActivateDoubleReward() { CrazyGamesManager.Instance.RewardedAdShow("DoubleGold"); }
    public void AdRevive() { CrazyGamesManager.Instance.RewardedAdShow("Revive"); }
    public void AdFreeGold() { CrazyGamesManager.Instance.RewardedAdShow("Gold"); }
    public void AdActivateSlow() { CrazyGamesManager.Instance.RewardedAdShow("SlowGame"); }
    public void AdFreeHealth() { CrazyGamesManager.Instance.RewardedAdShow("Health"); }
    public void RandomFreeUpgrade() { CrazyGamesManager.Instance.RewardedAdShow("RandomFreeUpgrade"); }

        private void AddGoldToWallet(int amount)
    {
        int currentMoney = PlayerPrefs.GetInt("TotalGold", 0);
        PlayerPrefs.SetInt("TotalGold", currentMoney + amount);
        PlayerPrefs.Save();
    }

    // --- REWARD GRANT METHODS (Reklam bittikten sonra arlan) ---
    public void GrantDoubleReward()
    {
        Debug.Log("Ödül ikiye katlanıyor...");

        AddGoldToWallet(lastCalculatedReward);
        goldText.text = (lastCalculatedReward * 2).ToString();

        if (adIconsGroup != null)
        {
            adIconsGroup.SetActive(false);
        }

        if (doubleRewardButton != null)
        {
            doubleRewardButton.interactable = false;
        }
    }

        public void GrantRevive() 
    { 
        VehicleStackManager.Instance.Revive(); 
        if (VehicleStackManager.Instance.gameOverPanel != null) VehicleStackManager.Instance.gameOverPanel.SetActive(false); 
        if (LevelManager.Instance != null && LevelManager.Instance.gameOverPanel != null) LevelManager.Instance.gameOverPanel.SetActive(false);
    }
    public void GrantFreeGold() { VehicleStackManager.Instance.AddMoney(300); RewardButtons.Instance.HideCurrent(); }
    public void GrantActivateSlow() { GameManager.Instance.SlowGame(15f); RewardButtons.Instance.HideCurrent(); }
    public void GrantFreeHealth() { VehicleStackManager.Instance.AddHealth(); RewardButtons.Instance.HideCurrent(); }
    public void GrantRandomFreeUpgrade() { UpgradeManager.Instance.ApplyRandomFreeUpgrade(); RewardButtons.Instance.HideCurrent(); PerkManager.Instance.perkPanel.SetActive(false); }
}



