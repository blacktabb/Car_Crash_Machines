using UnityEngine;
using CrazyGames; // Kütüphane ekli olmalý
using System;

public class CrazyGamesManager : MonoBehaviour
{
    public static CrazyGamesManager Instance; // Bu satýrda uyarýyý veriyor.

    // Inspector'dan atama yapmana artýk gerek yok ama kalmasýnda da sakýnca yok.
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

        CrazySDK.Init(() =>
        {
            Debug.Log("CrazyGames SDK Ready");
        });
    }

    // --- BUTON FONKSÝYONLARI ---


    // Ödüllü Reklam (Rewarded)
    public void RewardedAdShow(string rewardID)
    {
        chosenReward = rewardID;

        Debug.Log("Ödüllü reklam butonuna basýldý.");

        CrazySDK.Ad.RequestAd(CrazyAdType.Rewarded,
            () => {
                // Reklam Baþladý
                Debug.Log("Reklam baþladý. Oyun duruyor.");
                Time.timeScale = 0f;
            },
            (error) => {
                // Hata
                Debug.LogError("Reklam hatasý: " + error);
                Time.timeScale = 1f;
            },
            () => {
                // Reklam Bitti (Ödül Zamaný)
                Debug.Log("Reklam bitti.");
                Time.timeScale = 1f;            
                TakeReward();
                VehicleStackManager.Instance.gameOverPanel.SetActive(false);
                PerkManager.Instance.perkPanel.SetActive(false);
            }
        );
    }

    // Geçiþ Reklamý (Interstitial)
    public void ShowMidgameAd(Action onComplete = null)
    {
        Debug.Log("Geçiþ reklamý isteniyor...");

        CrazySDK.Ad.RequestAd(CrazyAdType.Midgame,
            () => {
                // Reklam Baþladý
                Time.timeScale = 0f;
            },
            (error) => {
                // Hata durumunda da oyunu devam ettirmeliyiz
                Debug.LogError("Geçiþ reklamý hatasý: " + error);
                Time.timeScale = 1f;
                if (onComplete != null) onComplete();
            },
            () => {
                // Reklam Bitti
                Debug.Log("Geçiþ reklamý bitti.");
                Time.timeScale = 1f;
                if (onComplete != null) onComplete();
            }
        );
    }

    void TakeReward()
    {     

        switch (chosenReward)
        {
            case "Revive":
                levelRewardManager.AdRevive();
                VehicleStackManager.Instance.gameOverPanel.SetActive(false);
                break;

            case "Gold":
                levelRewardManager.AdFreeGold();
                break;

            case "SlowGame":
                levelRewardManager.AdActivateSlow();
                break;

            case "Health":
                levelRewardManager.AdFreeHealth();
                break;

            case "DoubleGold":
                levelRewardManager.ActivateDoubleReward();
                break;

            case "RandomFreeUpgrade":
                levelRewardManager.RandomFreeUpgrade();
                PerkManager.Instance.perkPanel.SetActive(false);
                break;

            default:
                Debug.LogWarning("Bilinmeyen ödül türü: " + chosenReward);
                break;
        }
        Debug.Log("Tebrikler! Ödül hesabýna eklendi.");
    }
}