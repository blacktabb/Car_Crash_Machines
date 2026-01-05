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

        // 1. YATAY HAREKET (DÜZELTÝLDÝ)
        // Space.World ekleyerek taþ dönük olsa bile DÜNYA SOLUNA gitmesini saðladýk.
        float moveAmount = GameManager.Instance.gameSpeed * Time.deltaTime;

        transform.Translate(Vector3.left * moveAmount, Space.World);

        // 2. SINIR KONTROLÜ
        if (transform.position.x < destroyX)
        {
            Destroy(gameObject);
            return;
        }

        // 3. YERÇEKÝMÝ
        HandleGravity();
    }

    // ... (HandleGravity ve diðer fonksiyonlarýn aynen kalsýn) ...
    void HandleGravity()
    {
        float targetY = -50f;
        float distanceToGround;

        if (DetectGround(out distanceToGround))
        {
            targetY = transform.position.y - distanceToGround;
        }

        float newY = Mathf.MoveTowards(transform.position.y, targetY, fallSpeed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        if (Mathf.Abs(transform.position.y - targetY) < 0.001f) isGrounded = true;
        else isGrounded = false;
    }

    bool DetectGround(out float distance)
    {
        distance = 0f;
        Bounds bounds = col.bounds;
        float skinWidth = 0.05f;
        Vector3 leftFoot = new Vector3(bounds.min.x + skinWidth, bounds.min.y + 0.01f, bounds.center.z);
        Vector3 rightFoot = new Vector3(bounds.max.x - skinWidth, bounds.min.y + 0.01f, bounds.center.z);

        RaycastHit hitLeft, hitRight;
        bool isLeftHit = Physics.Raycast(leftFoot, Vector3.down, out hitLeft, 100f, obstacleLayer);
        bool isRightHit = Physics.Raycast(rightFoot, Vector3.down, out hitRight, 100f, obstacleLayer);

        float distLeft = (isLeftHit && hitLeft.collider.gameObject != gameObject) ? hitLeft.distance - 0.01f : 999f;
        float distRight = (isRightHit && hitRight.collider.gameObject != gameObject) ? hitRight.distance - 0.01f : 999f;

        float minDistance = Mathf.Min(distLeft, distRight);

        if (minDistance < 900f)
        {
            distance = minDistance;
            return true;
        }
        return false;
    }
}