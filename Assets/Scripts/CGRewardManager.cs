using UnityEngine;
using UnityEngine.UI;

public class CGRewardButton : MonoBehaviour
{
    public string rewardID;
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            CrazyGamesManager.Instance.RewardedAdShow(rewardID);
        });
    }
}
