using UnityEngine;

public class GridMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D col;

    [Header("Durum")]
    public bool isFalling = false;

    [Header("Ayarlar")]
    public float fallSpeed = 15f;
    public LayerMask supportLayers;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void FixedUpdate()
    {
        // 1. YATAY HAREKET
        float newX = rb.position.x - (GameManager.Instance.gameSpeed * Time.fixedDeltaTime);
        Vector2 nextPosition = new Vector2(newX, rb.position.y);

        // 2. DÝKEY HAREKET
        if (isFalling)
        {
            float moveDownAmount = fallSpeed * Time.fixedDeltaTime;

            // Ýniþ noktasýný hesapla (En yüksek engeli bul)
            float landingY = GetLandingSurfaceY(moveDownAmount);

            // Eðer landingY geçerli bir sayýysa (yani altýmýzda bir þey varsa)
            if (landingY != -999f)
            {
                // Yere yapýþtýr
                nextPosition.y = landingY;
                isFalling = false;
            }
            else
            {
                // Engel yok, düþmeye devam
                nextPosition.y -= moveDownAmount;
            }
        }
        else
        {
            // Düþmüyorsak: Altýmýzda HALA destek var mý?
            // Destek yoksa düþmeye baþla
            if (!CheckSupport())
            {
                isFalling = true;
            }
        }

        rb.MovePosition(nextPosition);
    }

    // --- DESTEK KONTROLÜ (Düþmeye baþlamalý mýyým?) ---
    bool CheckSupport()
    {
        // Ýki ayak kontrolü: Sol ve Sað
        // Geniþliðin %25'i içeriden atýyoruz ki tam sýnýrdakilere takýlmasýn
        float xOffset = (col.size.x * transform.localScale.x) / 4f;

        Vector2 leftOrigin = (Vector2)transform.position - new Vector2(xOffset, 0);
        Vector2 rightOrigin = (Vector2)transform.position + new Vector2(xOffset, 0);

        // Iþýn uzunluðu (Hemen altý)
        float rayLen = (col.size.y * transform.localScale.y) / 2f + 0.1f;

        RaycastHit2D leftHit = Physics2D.Raycast(leftOrigin, Vector2.down, rayLen, supportLayers);
        RaycastHit2D rightHit = Physics2D.Raycast(rightOrigin, Vector2.down, rayLen, supportLayers);

        // EÐER SOL VEYA SAÐ DOLUYSA -> DESTEK VARDIR
        if (leftHit.collider != null || rightHit.collider != null)
        {
            return true;
        }

        return false; // Ýkisi de boþsa düþ
    }

    // --- ÝNÝÞ NOKTASI HESABI (Nereye oturmalýyým?) ---
    // -999 dönerse "Daha yolun var, düþmeye devam et" demektir.
    float GetLandingSurfaceY(float checkDistance)
    {
        float xOffset = (col.size.x * transform.localScale.x) / 4f;
        float halfHeight = (col.size.y * transform.localScale.y) / 2f;

        // Raycast'ler merkezden deðil, objenin içinden baþlasýn (Hata payý için)
        float rayStartY = transform.position.y;

        Vector2 leftOrigin = new Vector2(transform.position.x - xOffset, rayStartY);
        Vector2 rightOrigin = new Vector2(transform.position.x + xOffset, rayStartY);

        // Ne kadar uzaða bakalým? (Yarým boy + düþülecek mesafe)
        float totalCheckDist = halfHeight + checkDistance;

        RaycastHit2D leftHit = Physics2D.Raycast(leftOrigin, Vector2.down, totalCheckDist, supportLayers);
        RaycastHit2D rightHit = Physics2D.Raycast(rightOrigin, Vector2.down, totalCheckDist, supportLayers);

        float foundSurfaceY = -999f;

        // Sol ayak bir þeye çarptý mý?
        if (leftHit.collider != null)
        {
            // Yüzeyin Y'si + Benim yarým boyum = Benim durmam gereken Merkez Y
            float potentialY = leftHit.point.y + halfHeight;
            if (potentialY > foundSurfaceY) foundSurfaceY = potentialY;
        }

        // Sað ayak bir þeye çarptý mý?
        if (rightHit.collider != null)
        {
            float potentialY = rightHit.point.y + halfHeight;
            // Eðer sað taraf daha yüksekse, orayý baz al (Ýç içe geçmemek için en yükseðe oturmalýyýz)
            if (potentialY > foundSurfaceY) foundSurfaceY = potentialY;
        }

        return foundSurfaceY;
    }

    void OnDrawGizmos()
    {
        if (col == null) return;

        // Gizmos ile ayaklarý görelim
        Gizmos.color = isFalling ? Color.red : Color.green;
        float xOffset = (col.size.x * transform.localScale.x) / 4f;
        float len = (col.size.y * transform.localScale.y) / 2f + 0.2f;

        Vector2 leftPos = (Vector2)transform.position - new Vector2(xOffset, 0);
        Vector2 rightPos = (Vector2)transform.position + new Vector2(xOffset, 0);

        Gizmos.DrawRay(leftPos, Vector2.down * len);
        Gizmos.DrawRay(rightPos, Vector2.down * len);
    }
}