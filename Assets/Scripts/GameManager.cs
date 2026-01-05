using UnityEngine;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // --- STATIC KONTROL ---
    public static bool isFirstLaunch = true;

    [Header("Baþlangýç UI Ayarlarý")]
    public GameObject tapToPlayPanel;
    public TextMeshProUGUI flashingText;
    public float blinkSpeed = 5f; // Animasyon hýzý (Daha hýzlý nefes alsýn diye artýrabilirsin)

    [Header("Oyun Hýzý")]
    public float targetSpeed = 5f;

    [HideInInspector]
    public float gameSpeed = 0f;

    private bool waitingForInput = false;
    Coroutine slowRoutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (isFirstLaunch)
        {
            gameSpeed = 0f;
            waitingForInput = true;

            if (tapToPlayPanel != null)
                tapToPlayPanel.SetActive(true);
        }
        else
        {
            gameSpeed = targetSpeed;
            waitingForInput = false;

            if (tapToPlayPanel != null)
                tapToPlayPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (waitingForInput)
        {
            HandleTapToPlay();
        }
    }

    void HandleTapToPlay()
    {
        // --- DEÐÝÞÝKLÝK BURADA: PULSE (BÜYÜME-KÜÇÜLME) EFEKTÝ ---
        if (flashingText != null)
        {
            // Mantýk: 
            // Mathf.Sin -> -1 ile 1 arasýnda gidip gelen bir dalga üretir.
            // * 0.1f -> Bu dalgayý küçültürüz (-0.1 ile 0.1 arasý).
            // + 1f   -> Üzerine 1 ekleriz (0.9 ile 1.1 arasý olur).
            // Sonuç: Metin orijinal boyutunun %90'ý ile %110'u arasýnda gidip gelir.

            float scaleValue = 1f + (Mathf.Sin(Time.time * blinkSpeed) * 0.1f);

            // UI elemanýnýn boyutunu (Scale) güncelle
            flashingText.transform.localScale = Vector3.one * scaleValue;
        }

        // 2. Input: Týklama Algýlama
        if (Input.GetMouseButtonDown(0))
        {
            StartGameLogic();
        }
    }

    public void StartGameLogic()
    {
        waitingForInput = false;
        isFirstLaunch = false;
        gameSpeed = targetSpeed;

        if (tapToPlayPanel != null)
            tapToPlayPanel.SetActive(false);
    }

    // --- HIZ YAVAÞLATMA SÝSTEMÝ ---
    public void SlowGame(float duration)
    {
        if (slowRoutine != null)
            StopCoroutine(slowRoutine);

        slowRoutine = StartCoroutine(SlowRoutine(duration));
    }

    IEnumerator SlowRoutine(float duration)
    {
        float originalSpeed = targetSpeed;

        gameSpeed = originalSpeed * 0.5f;
        yield return new WaitForSeconds(duration);

        if (!waitingForInput)
            gameSpeed = originalSpeed;

        slowRoutine = null;
    }
}