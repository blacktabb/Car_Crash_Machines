using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CoinAnimationManager : MonoBehaviour
{
    public static CoinAnimationManager Instance;

    [Header("Ayarlar")]
    [SerializeField] private GameObject coinPrefab; // UI Image Prefabý (Mutlaka bir UI Image olmalý!)
    [SerializeField] private Transform targetLogo;  // Hedef (Sað üstteki altýn ikonu)
    [SerializeField] private Transform mainCanvas;  // Canvas (Coinlerin içinde oluþacaðý yer)

    [Header("Animasyon Ayarlarý")]
    [SerializeField] private int maxCoinAmount = 5;
    [SerializeField] private float spread = 100f;
    [SerializeField] private float speed = 1.5f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayCoinAnim(Vector3 worldStartPos, int amount)
    {
        // 1. 3D Dünya Pozisyonunu (Taþýn yeri) Ekran Pozisyonuna Çevir
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldStartPos);

        // ÖNEMLÝ: Z eksenini 0 yapýyoruz ki UI'ýn arkasýnda veya önünde kaybolmasýn.
        screenPos.z = 0;

        Debug.Log("Coin Animasyonu Baþladý! Baþlangýç: " + screenPos);

        int coinCount = Mathf.Min(amount, maxCoinAmount);
        if (coinCount < 3) coinCount = 3;

        for (int i = 0; i < coinCount; i++)
        {
            CreateAndMoveCoin(screenPos);
        }
    }

    // Hedef objeyi (Altýn Ýkonunu) büyütüp küçülten efekt
    IEnumerator PunchEffect(Transform target)
    {
        // 1. Anlýk olarak büyüt (1.2 katýna çýkar)
        target.localScale = new Vector3(1.1f, 1.1f, 1.1f);

        // 2. Çok kýsa bekle (0.05 saniye)
        yield return new WaitForSeconds(0.1f);

        // 3. Tekrar normal boyutuna döndür
        target.localScale = Vector3.one;
    }

    void CreateAndMoveCoin(Vector3 startPos)
    {
        // Coin oluþtur
        GameObject coin = Instantiate(coinPrefab, mainCanvas);

        // ÖNEMLÝ: UI olduðu için RectTransform kullanýyoruz
        RectTransform rect = coin.GetComponent<RectTransform>();

        // Pozisyonu ayarla
        coin.transform.position = startPos;

        // ÖNEMLÝ: Boyutlarý sýfýrla (Bazen instantiate edince dev gibi veya minik oluyor)
        coin.transform.localScale = Vector3.one;

        // Rastgele bir daðýlma pozisyonu belirle
        Vector3 scatterPos = startPos + (Vector3)(Random.insideUnitCircle * spread);
        scatterPos.z = 0;

        StartCoroutine(MoveCoinRoutine(coin.transform, scatterPos));
    }

    IEnumerator MoveCoinRoutine(Transform coinTrans, Vector3 scatterPos)
    {
        // 1. AÞAMA: PATLAMA (Etrafa saçýlma)
        float t = 0;
        Vector3 startPos = coinTrans.position;

        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            coinTrans.position = Vector3.Lerp(startPos, scatterPos, t);
            yield return null;
        }

        // 2. AÞAMA: HEDEFE GÝTME
        t = 0;
        Vector3 currentPos = coinTrans.position;

        // Hedefin (UI Ýkonunun) pozisyonunu al
        Vector3 targetPos = targetLogo.position;
        targetPos.z = 0; // Hedefin de Z'sini sýfýrla

        while (t < 1f)
        {
            if (coinTrans == null) break;

            t += Time.deltaTime * speed;
            // Hýzlanan bir hareket (Ease-In)
            coinTrans.position = Vector3.Lerp(currentPos, targetPos, t * t);

            if (Vector3.Distance(coinTrans.position, targetPos) < 50f)
            {
                break;
            }
            yield return null;
        }

        if (targetLogo != null)
        {
            StartCoroutine(PunchEffect(targetLogo));
        }

        if (coinTrans != null)
            Destroy(coinTrans.gameObject);

        // Burada ses çaldýrabilirsin: AudioSource.PlayClipAtPoint(...)
    }
}