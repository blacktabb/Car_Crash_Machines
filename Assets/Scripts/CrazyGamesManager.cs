using UnityEngine;
using System;
using CrazyGames;

public class CrazyGamesManager : MonoBehaviour
{
    public static CrazyGamesManager Instance;

    [SerializeField] public LevelRewardManager levelRewardManager;
    private string chosenReward;
    private Action currentInterstitialCallback;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CrazySDK.Init(() =>
        {
            Debug.Log("[CrazyGames] SDK Ready");
        });
    }

    public void RewardedAdShow(string rewardID)
    {
        chosenReward = rewardID;

        CrazySDK.Ad.RequestAd(
            CrazyAdType.Rewarded,
            () =>
            {
                Time.timeScale = 0f;
                if (AudioManager.Instance != null) AudioManager.Instance.SetSFXState(false);
            },
            (error) =>
            {
                Debug.Log("[CrazyGames] Rewarded ad error: " + error);
                Time.timeScale = 1f;
                if (AudioManager.Instance != null)
                {
                    bool isSoundOn = PlayerPrefs.GetInt("Sound", 1) == 1;
                    AudioManager.Instance.SetSFXState(isSoundOn);
                }
            },
            () =>
            {
                Time.timeScale = 1f;
                if (AudioManager.Instance != null)
                {
                    bool isSoundOn = PlayerPrefs.GetInt("Sound", 1) == 1;
                    AudioManager.Instance.SetSFXState(isSoundOn);
                }
                TakeReward();
                if (VehicleStackManager.Instance != null && VehicleStackManager.Instance.gameOverPanel != null)
                    VehicleStackManager.Instance.gameOverPanel.SetActive(false);
                if (PerkManager.Instance != null && PerkManager.Instance.perkPanel != null)
                    PerkManager.Instance.perkPanel.SetActive(false);
            }
        );
    }

    public void ShowMidgameAd(Action onComplete = null)
    {
        currentInterstitialCallback = onComplete;

        CrazySDK.Ad.RequestAd(
            CrazyAdType.Midgame,
            () =>
            {
                Time.timeScale = 0f;
                if (AudioManager.Instance != null) AudioManager.Instance.SetSFXState(false);
            },
            (error) =>
            {
                Debug.Log("[CrazyGames] Midgame ad error: " + error);
                Time.timeScale = 1f;
                if (AudioManager.Instance != null)
                {
                    bool isSoundOn = PlayerPrefs.GetInt("Sound", 1) == 1;
                    AudioManager.Instance.SetSFXState(isSoundOn);
                }
                currentInterstitialCallback?.Invoke();
                currentInterstitialCallback = null;
            },
            () =>
            {
                Time.timeScale = 1f;
                if (AudioManager.Instance != null)
                {
                    bool isSoundOn = PlayerPrefs.GetInt("Sound", 1) == 1;
                    AudioManager.Instance.SetSFXState(isSoundOn);
                }
                currentInterstitialCallback?.Invoke();
                currentInterstitialCallback = null;
            }
        );
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
