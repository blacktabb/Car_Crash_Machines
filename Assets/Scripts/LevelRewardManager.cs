using UnityEngine;
using TMPro;

public class LevelRewardManager : MonoBehaviour
{
    [Header("UI Elemanlarý")]
    public TextMeshProUGUI goldText;

    [Header("Ayarlar")]
    public int baseGold = 50;
    public int increaseAmount = 50;
    public int penaltyPerHealth = 10;

    public void ShowReward(int level, int currentHealth, int maxHealth)
    {
        // 1. Ödül Hesabý
        int levelMultiplier = level / 5;
        int calculatedBaseGold = baseGold + (levelMultiplier * increaseAmount);
        int missingHealth = maxHealth - currentHealth;
        int penalty = missingHealth * penaltyPerHealth;
        int finalReward = Mathf.Max(0, calculatedBaseGold - penalty);

        // 2. UI Güncelleme
        goldText.text = finalReward.ToString();

        // 3. Parayý Kaydet
        int currentMoney = PlayerPrefs.GetInt("TotalGold", 0);
        PlayerPrefs.SetInt("TotalGold", currentMoney + finalReward);
        PlayerPrefs.Save();

    }
}