using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollArrowIndicator : MonoBehaviour
{
    [Header("Ok Objeleri")]
    public GameObject upArrow;
    public GameObject downArrow;

    [Header("Ayarlar")]
    [Tooltip("En tepeye veya en dibe ne kadar yaklaþýldýðýnda oklar kaybolsun? (0 ile 1 arasý)")]
    public float threshold = 0.05f;

    private ScrollRect scrollRect;

    void Start()
    {
        scrollRect = GetComponent<ScrollRect>();

        // Scroll her kaydýrýldýðýnda CheckArrows fonksiyonunu çalýþtýr
        scrollRect.onValueChanged.AddListener(CheckArrows);

        // Oyun baþladýðýnda UI hesaplamalarýnýn bitmesini çok kýsa bir süre bekliyoruz
        Invoke(nameof(ForceInitialCheck), 0.05f);
    }

    void ForceInitialCheck()
    {
        if (scrollRect != null)
        {
            // Liste tam boyutuna ulaþtýktan sonra ilk kontrolü yap
            CheckArrows(scrollRect.normalizedPosition);
        }
    }

    void CheckArrows(Vector2 scrollPosition)
    {
        // 1 = En Üst, 0 = En Alt
        float currentPos = scrollRect.verticalNormalizedPosition;

        // GÜVENLÝK KONTROLÜ: Eðer listenin boyu, görünür alandan (Viewport) daha kýsaysa 
        // kaydýrmaya gerek yoktur. Oklarý tamamen gizle.
        if (scrollRect.content.rect.height <= scrollRect.viewport.rect.height)
        {
            if (upArrow != null) upArrow.SetActive(false);
            if (downArrow != null) downArrow.SetActive(false);
            return;
        }

        // YUKARI OK: Eðer en üstte deðilsek (1'den küçüksek) göster
        if (upArrow != null)
        {
            upArrow.SetActive(currentPos < 1f - threshold);
        }

        // AÞAÐI OK: Eðer en altta deðilsek (0'dan büyüksek) göster
        if (downArrow != null)
        {
            downArrow.SetActive(currentPos > threshold);
        }
    }
}