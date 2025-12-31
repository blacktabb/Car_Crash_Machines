using UnityEngine;

public class StoneController : MonoBehaviour
{
    [Header("Ayarlar")]
    // Not: moveSpeed artýk GameManager'dan alýnýyor.
    public float fallSpeed = 10f;
    public float destroyX = -30f; // Ekrandan çýkma sýnýrý

    [Header("Fizik & Grid")]
    public Vector2 stoneSize = new Vector2(1, 1); // Inspector'dan ayarla! (Küçük: 1,1 | Büyük: 2,2)
    public LayerMask obstacleLayer; // Ground ve Stone seçili olmalý

    private bool isGrounded = false;
    private BoxCollider col;

    void Awake()
    {
        col = GetComponent<BoxCollider>();
        // Rigidbody'i Kinematic yapýyoruz ki fizik motoru sapýtmasýn
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        // 1. YATAY HAREKET (Sola Git - Global Hýzla)
        float moveAmount = GameManager.Instance.gameSpeed * Time.deltaTime;
        transform.Translate(Vector3.left * moveAmount);

        // 2. SINIR KONTROLÜ
        if (transform.position.x < destroyX)
        {
            Destroy(gameObject);
            return;
        }

        // 3. YERÇEKÝMÝ KONTROLÜ
        HandleGravity();
    }

    void HandleGravity()
    {
        // Eðer zaten yerdeysek, altýmýz boþaldý mý diye kontrol et (Örn: Alttaki taþ patladýysa)
        if (isGrounded)
        {
            if (!CheckGround()) isGrounded = false;
            return;
        }

        // --- DÜÞME ÝÞLEMÝ ---
        float fallDist = fallSpeed * Time.deltaTime;

        // Yere ne kadar mesafe var?
        float distToGround = GetDistanceToGround();

        if (distToGround <= fallDist) // Çarpmak üzereyiz
        {
            // Tam zemine yapýþtýr (Snap)
            transform.position = new Vector3(transform.position.x, transform.position.y - distToGround, transform.position.z);
            isGrounded = true;

            // Titremeyi önlemek için Y pozisyonunu tam sayýya/buçuða yuvarla
            SnapYPosition();
        }
        else
        {
            // Henüz havada, düþmeye devam
            transform.Translate(Vector3.down * fallDist);
        }
    }

    // Raycast ile altýmýzý ölçüyoruz
    float GetDistanceToGround()
    {
        Vector3 center = transform.position;
        // Hafif daraltýlmýþ boxcast (Yan duvarlara sürtmesin diye)
        Vector3 size = new Vector3(stoneSize.x * 0.9f, 0.1f, 1f);

        RaycastHit hit;
        // Taþýn merkezinden aþaðý doðru tarýyoruz
        float checkDist = 100f; // Sonsuza kadar bakabilir ama 100 yeterli

        // Raycast'in baþlangýç noktasý taþýn alt kenarý olsun
        float halfHeight = stoneSize.y * 0.5f;
        Vector3 origin = center;

        if (Physics.BoxCast(origin, size * 0.5f, Vector3.down, out hit, Quaternion.identity, checkDist, obstacleLayer))
        {
            // Kendimize çarpmayalým
            if (hit.collider.gameObject != gameObject)
            {
                // Mesafe: (Merkezden vuruþa olan mesafe) - (Yarým boy)
                return hit.distance - halfHeight;
            }
        }
        return 999f; // Alt boþ
    }

    bool CheckGround()
    {
        return GetDistanceToGround() < 0.1f;
    }

    void SnapYPosition()
    {
        Vector3 pos = transform.position;
        if (stoneSize.y % 2 != 0) // Tek sayý (1, 3) -> Tam sayýya yuvarla
            pos.y = Mathf.Round(pos.y);
        else // Çift sayý (2, 4) -> Buçuða yuvarla
            pos.y = Mathf.Floor(pos.y) + 0.5f;

        transform.position = pos;
    }
}