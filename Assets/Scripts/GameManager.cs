using UnityEngine;
using System.Collections;
using TMPro;
using Playgama;
using Playgama.Modules.Platform;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // --- STATIC KONTROL ---
    public static bool isFirstLaunch = true;

    [Header("Başlangıç UI Ayarları")]
    public GameObject tapToPlayPanel;
    public TextMeshProUGUI flashingText;
    public float blinkSpeed = 5f; // Animasyon hızı (Daha hızlı nefes alsın diye artırabilirsin)

    [Header("Oyun Hızı")]
    public float targetSpeed = 5f;
    public float maxTargetSpeed = 12f; // --- YENİ: Maksimum Hız Sınırı ---

    [HideInInspector]
    public float gameSpeed = 0f;

    private bool waitingForInput = false;
    Coroutine slowRoutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        string currentVersion = Application.version;
        string savedVersion = PlayerPrefs.GetString("GameVersion", "");

        if (savedVersion != currentVersion)
        {
            Debug.Log("Yeni build tespit edildi. PlayerPrefs temizleniyor.");

            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetString("GameVersion", currentVersion);
            PlayerPrefs.Save();
        }
    }

    void Start()
    {
        Bridge.platform.SendMessage(PlatformMessage.GameReady);
        // Başlangıçta hedef hızın sınırı aşmadığından emin ol
        targetSpeed = Mathf.Min(targetSpeed, maxTargetSpeed);

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
        // --- YENİ: HIZ SABİTLEME (CLAMP) ---
        // Eğer targetSpeed bir şekilde sınırı geçerse, onu sınır değerine geri çekiyoruz.
        if (targetSpeed > maxTargetSpeed)
        {
            targetSpeed = maxTargetSpeed;
        }

        // Eğer oyun akıyorsa ve bir yavaşlatma efekti aktif değilse, 
        // oyun hızını hedef hıza eşitliyoruz. (Böylece dışarıdan artışlar anında yansır)
        if (!waitingForInput && slowRoutine == null)
        {
            gameSpeed = targetSpeed;
        }
        // -----------------------------------

        if (waitingForInput)
        {
            HandleTapToPlay();
        }
    }

    void HandleTapToPlay()
    {
        if (flashingText != null)
        {
            float scaleValue = 1f + (Mathf.Sin(Time.time * blinkSpeed) * 0.1f);
            flashingText.transform.localScale = Vector3.one * scaleValue;
        }

        if (Input.GetMouseButtonDown(0))
        {
            StartGameLogic();
        }
    }

    public void StartGameLogic()
    {
        Bridge.platform.SendMessage(PlatformMessage.GameplayStarted);
        waitingForInput = false;
        isFirstLaunch = false;

        // Başlarken de sınırı koruyalım
        gameSpeed = Mathf.Min(targetSpeed, maxTargetSpeed);

        if (tapToPlayPanel != null)
            tapToPlayPanel.SetActive(false);
    }

    // --- HIZ YAVAŞLATMA SİSTEMİ ---
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
            gameSpeed = targetSpeed; // originalSpeed yerine direkt güncel targetSpeed'e dön

        slowRoutine = null;
    }
}
