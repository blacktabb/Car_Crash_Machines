using UnityEngine;

public class StoneController : MonoBehaviour
{
    [Header("Ayarlar")]
    public float fallSpeed = 10f;
    public float destroyX = -30f;

    [Header("Fizik")]
    public LayerMask obstacleLayer;

    private bool isGrounded = false;
    private BoxCollider col;
    private Rigidbody rb;

    void Awake()
    {
        col = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        // 1. YATAY HAREKET
        float moveAmount = GameManager.Instance.gameSpeed * Time.deltaTime;
        transform.Translate(Vector3.left * moveAmount);

        // 2. SINIR KONTROLÜ
        if (transform.position.x < destroyX)
        {
            Destroy(gameObject);
            return;
        }

        // 3. YERÇEKÝMÝ
        HandleGravity();
    }

    void HandleGravity()
    {
        // 1. Hedef Yüksekliði Belirle
        // Varsayýlan olarak sonsuza kadar düþebiliriz (-100 diyelim)
        float targetY = -50f;

        float distanceToGround;

        // Altýmýzda zemin var mý?
        if (DetectGround(out distanceToGround))
        {
            // Varsa hedefimiz: Þu anki Yeri - Mesafe
            // Yani tam zeminin üzerine oturacaðýmýz koordinat.
            targetY = transform.position.y - distanceToGround;
        }

        // 2. Oraya Doðru "Yumuþakça" Ýlerle
        // MoveTowards: Mevcut konumdan hedef konuma, verilen hýzla ilerler.
        // Hedefe vardýysa daha fazla gitmez (Otomatik fren).

        float newY = Mathf.MoveTowards(transform.position.y, targetY, fallSpeed * Time.deltaTime);

        // 3. Pozisyonu Güncelle
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // 4. Durduk mu? (Hedefe vardýk mý?)
        // Epsilon (çok küçük sayý) karþýlaþtýrmasý yapýyoruz
        if (Mathf.Abs(transform.position.y - targetY) < 0.001f)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    // Bu fonksiyon taþýn en alt noktasýndan aþaðýya ýþýn atar.
    // Taþýn boyutunu ve pivotunu otomatik hesaplar.
    bool DetectGround(out float distance)
    {
        distance = 0f;

        // Collider'ýn gerçek dünya sýnýrlarýný al
        Bounds bounds = col.bounds;

        // Taþýn sol alt ve sað alt köþelerini bul (Biraz içeriden)
        // Böylece yan duvara sürtünme sorunu olmaz.
        float skinWidth = 0.05f; // Kenarlardan ne kadar içeride olsun?

        Vector3 leftFoot = new Vector3(bounds.min.x + skinWidth, bounds.min.y + 0.01f, bounds.center.z);
        Vector3 rightFoot = new Vector3(bounds.max.x - skinWidth, bounds.min.y + 0.01f, bounds.center.z);

        // Aþaðý doðru ýþýn at (Raycast)
        RaycastHit hitLeft, hitRight;
        bool isLeftHit = Physics.Raycast(leftFoot, Vector3.down, out hitLeft, 100f, obstacleLayer);
        bool isRightHit = Physics.Raycast(rightFoot, Vector3.down, out hitRight, 100f, obstacleLayer);

        // Kendi colliderýmýza çarpmayý engellemek için ray'i bounds.min.y'den baþlattýk ama
        // bazen Physics motoru çok hassas olabilir. Mesafeyi ray baþlangýcýna göre alacaðýz.

        float distLeft = 999f;
        float distRight = 999f;

        // Sol ayak çarptý mý? (Kendimize çarpmadýðýmýzdan emin olalým)
        if (isLeftHit && hitLeft.collider.gameObject != gameObject)
            distLeft = hitLeft.distance - 0.01f; // 0.01f yukarýdan baþlatmýþtýk, onu düþüyoruz

        // Sað ayak çarptý mý?
        if (isRightHit && hitRight.collider.gameObject != gameObject)
            distRight = hitRight.distance - 0.01f;

        // Hangisi daha yakýnsa o mesafeyi al (En yüksek zemini kabul et)
        float minDistance = Mathf.Min(distLeft, distRight);

        if (minDistance < 900f) // Geçerli bir çarpýþma varsa
        {
            distance = minDistance;
            return true;
        }

        return false;
    }

    bool CheckGround()
    {
        float dist;
        // Eðer zemin 0.1 birimden daha yakýnsa yerdeyizdir
        if (DetectGround(out dist))
        {
            return dist < 0.1f;
        }
        return false;
    }

    // Gizmos ile Raycastleri sahnede görebilirsin (Debug için)
    void OnDrawGizmos()
    {
        if (col == null) col = GetComponent<BoxCollider>();
        if (col != null)
        {
            Gizmos.color = Color.red;
            Bounds bounds = col.bounds;
            float skinWidth = 0.05f;
            Vector3 leftFoot = new Vector3(bounds.min.x + skinWidth, bounds.min.y + 0.01f, bounds.center.z);
            Vector3 rightFoot = new Vector3(bounds.max.x - skinWidth, bounds.min.y + 0.01f, bounds.center.z);

            Gizmos.DrawRay(leftFoot, Vector3.down * 2f);
            Gizmos.DrawRay(rightFoot, Vector3.down * 2f);
        }
    }
}