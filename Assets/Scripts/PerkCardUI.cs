using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PerkCardUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public Button selectButton;

    private PerkManager.PerkData myData;
    private PerkManager manager;

    public void Setup(PerkManager.PerkData data, PerkManager mgr)
    {
        myData = data;
        manager = mgr;

        titleText.text = data.title;
        descText.text = data.description;
        if (data.icon != null) iconImage.sprite = data.icon;

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnSelect);
    }

    void OnSelect()
    {
        manager.ApplyPerk(myData);
    }
}