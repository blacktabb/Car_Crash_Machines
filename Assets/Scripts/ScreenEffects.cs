using UnityEngine;
using UnityEngine.UI;

public class ScreenEffectsController : MonoBehaviour
{
    [Header("UI Referanslarý")]
    public Image redVignette;
    public Image whiteVignette;

    [Header("Mesafe Ayarlarý (Kýrmýzý)")]
    public float dangerStartDistance = 15f; // Tarama yarýçapý
    public float maxDangerDistance = 3f;    // En kýrmýzý olacaðý mesafe
    [Range(0, 1)] public float maxRedAlpha = 0.6f;

    [Header("Can Ayarlarý (Beyaz)")]
    public float pulseSpeed = 5f;
    [Range(0, 1)] public float maxWhiteAlpha = 0.5f;

    // Performans için LayerMask (Sadece belirli katmanlarý tara)
    public LayerMask obstacleLayer;

    private Transform playerTransform;

    void Start()
    {
        if (VehicleStackManager.Instance != null)
        {
            playerTransform = VehicleStackManager.Instance.transform;
        }

        SetAlpha(redVignette, 0);
        SetAlpha(whiteVignette, 0);
    }

    void Update()
    {
        HandleRedEffect();
        HandleWhiteEffect();
    }

    // --- 1. KIRMIZI EFEKT (YENÝLENMÝÞ VERSÝYON) ---
    void HandleRedEffect()
    {
        if (playerTransform == null) return;

        // Oyuncunun etrafýnda "dangerStartDistance" kadar bir küre çiz ve içindekileri bul
        // Bu yöntem X, Y, Z fark etmeksizin her yöndeki tehlikeyi bulur.
        Collider[] hitColliders = Physics.OverlapSphere(playerTransform.position, dangerStartDistance, obstacleLayer);

        float closestDistance = float.MaxValue;
        bool stoneFound = false;

        foreach (var hit in hitColliders)
        {
            // Sadece "Stone" tag'i olanlara bak
            if (hit.CompareTag("Stone"))
            {
                // Fiziksel mesafeyi hesapla (Vector3.Distance en garanti yoldur)
                float distance = Vector3.Distance(playerTransform.position, hit.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    stoneFound = true;
                }
            }
        }

        float targetAlpha = 0f;

        if (stoneFound)
        {
            // Mesafe azaldýkça Alpha artar
            float t = Mathf.InverseLerp(dangerStartDistance, maxDangerDistance, closestDistance);
            targetAlpha = t * maxRedAlpha;
        }

        // Yumuþak geçiþ (Titremeyi önler)
        float currentAlpha = redVignette.color.a;
        float smoothedAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * 10f);

        SetAlpha(redVignette, smoothedAlpha);
    }

    // --- 2. BEYAZ EFEKT (AYNI) ---
    void HandleWhiteEffect()
    {
        if (VehicleStackManager.Instance == null) return;

        // VehicleStackManager'a "public int GetHealth()" eklediðini varsayýyorum.
        // Eðer eklemediysen direkt .currentHealth (public ise) kullan.
        int currentHP = VehicleStackManager.Instance.GetHealth();

        if (currentHP == 1)
        {
            float alpha = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            SetAlpha(whiteVignette, alpha * maxWhiteAlpha);
        }
        else
        {
            SetAlpha(whiteVignette, 0);
        }
    }

    void SetAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    // Editörde tarama alanýný görmek için (Gizmos)
    void OnDrawGizmosSelected()
    {
        if (VehicleStackManager.Instance != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(VehicleStackManager.Instance.transform.position, dangerStartDistance);
        }
    }
}