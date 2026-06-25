using UnityEngine;
using System;

public class CrazyGamesManager : MonoBehaviour
{
    public static CrazyGamesManager Instance;

    [SerializeField] public LevelRewardManager levelRewardManager;
    private string chosenReward;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RewardedAdShow(string rewardID)
    {
        chosenReward = rewardID;
        // No SDK active on this branch
    }

    public void ShowMidgameAd(Action onComplete = null)
    {
        onComplete?.Invoke();
    }

    void TakeReward()
    {
        if (levelRewardManager == null) return;

        switch (chosenReward)
        {
            case "Revive":
                levelRewardManager.GrantRevive();
                if (VehicleStackManager.Instance != null && VehicleStackManager.Instance.gameOverPanel != null)
                    VehicleStackManager.Instance.gameOverPanel.SetActive(false);
                break;

            case "Gold":
                levelRewardManager.GrantFreeGold();
                break;

            case "SlowGame":
                levelRewardManager.GrantActivateSlow();
                break;

            case "Health":
                levelRewardManager.GrantFreeHealth();
                break;

            case "DoubleGold":
                levelRewardManager.GrantDoubleReward();
                break;

            case "RandomFreeUpgrade":
                levelRewardManager.GrantRandomFreeUpgrade();
                if (PerkManager.Instance != null && PerkManager.Instance.perkPanel != null)
                    PerkManager.Instance.perkPanel.SetActive(false);
                break;

            default:
                Debug.LogWarning("Bilinmeyen ödül türü: " + chosenReward);
                break;
        }
        Debug.Log("Tebrikler! Ödül hesabına eklendi.");
    }
}
