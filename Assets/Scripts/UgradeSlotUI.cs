using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeSlotUI : MonoBehaviour
{
    [Header("Kart Ýçindeki UI Elemanlarý")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI priceText; // Artýk hem yazýyý hem ikonu bu gösterecek
    public Button buyButton;
    public Image buttonBackground;

    [Header("Görsel Ayarlarý")]
    public Sprite affordableSprite;
    public Sprite unaffordableSprite;

    public void UpdateSlot(int level, string description, int price, bool canAfford)
    {
        // 1. Yazýlarý Güncelle
        if (levelText != null) levelText.text = "LEVEL " + (level + 1);
        if (descriptionText != null) descriptionText.text = description;

        // --- DEÐÝÞÝKLÝK BURADA ---
        // Fiyatýn yanýna <sprite=0> etiketini ekliyoruz.
        // Bu etiket, TextMeshPro'ya atadýðýn Sprite Asset'in 0. indeksindeki resmi (Altýn) çizer.
        if (priceText != null)
        {
            priceText.text = $"{price} <sprite=0>";
        }
        // -------------------------

        // 2. Buton ve Sprite Ayarlarý
        if (buyButton != null && buttonBackground != null)
        {
            buyButton.interactable = canAfford;

            // Sprite deðiþimi ve Renk sýfýrlama
            buttonBackground.sprite = canAfford ? affordableSprite : unaffordableSprite;
            buttonBackground.color = Color.white;
        }
    }
}