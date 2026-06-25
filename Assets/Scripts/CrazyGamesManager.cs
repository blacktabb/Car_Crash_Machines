using UnityEngine;
using System;
using Playgama;
using Playgama.Modules.Advertisement;

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
    }

    private void Start()
    {
        Bridge.advertisement.interstitialStateChanged += OnInterstitialStateChanged;
        Bridge.advertisement.rewardedStateChanged += OnRewardedStateChanged;
    }

    private void OnDestroy()
    {
        Bridge.advertisement.interstitialStateChanged -= OnInterstitialStateChanged;
        Bridge.advertisement.rewardedStateChanged -= OnRewardedStateChanged;
    }

    private void OnInterstitialStateChanged(InterstitialState state)
    {
        Debug.Log($"[Playgama] Interstitial State: {state}");
        if (state == InterstitialState.Opened)
        {
            Time.timeScale = 0f;
            if (AudioManager.Instance != null) AudioManager.Instance.SetSFXState(false);
        }
        else if (state == InterstitialState.Closed || state == InterstitialState.Failed)
        {
            Time.timeScale = 1f;
            if (AudioManager.Instance != null) {
                bool isSoundOn = PlayerPrefs.GetInt("Sound", 1) == 1;
                AudioManager.Instance.SetSFXState(isSoundOn);
            }
            if (currentInterstitialCallback != null)
            {
                currentInterstitialCallback();
                currentInterstitialCallback = null;
            }
        }
    }

    private void OnRewardedStateChanged(RewardedState state)
    {
        Debug.Log($"[Playgama] Rewarded State: {state}");
        if (state == RewardedState.Opened)
        {
            Time.timeScale = 0f;
            if (AudioManager.Instance != null) AudioManager.Instance.SetSFXState(false);
        }
        else if (state == RewardedState.Closed || state == RewardedState.Failed)
        {
            Time.timeScale = 1f;
            if (AudioManager.Instance != null) {
                bool isSoundOn = PlayerPrefs.GetInt("Sound", 1) == 1;
                AudioManager.Instance.SetSFXState(isSoundOn);
            }
        }
        else if (state == RewardedState.Rewarded)
        {
            TakeReward();
            if (VehicleStackManager.Instance != null && VehicleStackManager.Instance.gameOverPanel != null)
                VehicleStackManager.Instance.gameOverPanel.SetActive(false);
                
            if (PerkManager.Instance != null && PerkManager.Instance.perkPanel != null)
                PerkManager.Instance.perkPanel.SetActive(false);
        }
    }

        public void RewardedAdShow(string rewardID)
    {
        chosenReward = rewardID;

        // SDK'ya doğrudan çağrı yapıyoruz, desteklenmeyen ortamlarda (Unity Editor vs.) 
        // Playgama kendi Mock UI'sini (Test ekranını) gösterebilir.
        Bridge.advertisement.ShowRewarded();
    }

        public void ShowMidgameAd(Action onComplete = null)
    {
        currentInterstitialCallback = onComplete;
        
        // SDK'ya doğrudan çağrı yapıyoruz
        Bridge.advertisement.ShowInterstitial();
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


