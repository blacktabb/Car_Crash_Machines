using UnityEngine;
using CrazyGames; // Kütüphane ekli olmalý
using System;

public class CrazyGamesManager : MonoBehaviour
{
    // Butona baðlayacaðýn fonksiyon (Ödüllü Reklam)
    [SerializeField] public LevelRewardManager levelRewardManager;
    private string chosenReward;
    public static CrazyGamesManager Instance;
    // Oyun açýlýr açýlmaz (Start'tan bile önce) burasý çalýþýr
    private void Awake()
    {
        Debug.Log("CrazyGames SDK baþlatýlýyor...");

        // SDK'yý manuel olarak baþlatýyoruz
        CrazySDK.Init(() =>
        {
            // SDK baþarýyla kuruldu, artýk reklam istenebilir
            Debug.Log("CrazyGames SDK Baþarýyla Baþlatýldý (Init Tamam)!");
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
                break;

            default:
                Debug.LogWarning("Bilinmeyen ödül türü: " + chosenReward);
                break;                      
        }        
        Debug.Log("Tebrikler! Ödül hesabýna eklendi.");
    }
}