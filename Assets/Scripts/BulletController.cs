using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("Mermi Ayarlarý")]
    public float speed = 10f;
    public float maxRange = 15f;

    private Vector3 startPosition;
    private float damage;
    private bool isCriticalHit = false;

    public GameObject damagePopupPrefab;

    void Start()
    {
        startPosition = transform.position;
    }

    public void SetDamage(float amount, bool isCritical)
    {
        damage = amount;
        isCriticalHit = isCritical;
    }

    void Update()
    {
        // Oyun durduysa hareket etme
        if (GameManager.Instance == null || GameManager.Instance.gameSpeed <= 0f)
            return;

        // --- DÜZELTME BURADA ---
        // Space.World ekleyerek merminin dönüþü ne olursa olsun
        // DÜNYA KOORDÝNATLARINDA SAÐA gitmesini saðlýyoruz.
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.World);
        // -----------------------

        // Menzil Kontrolü
        float distanceTraveled = Vector3.Distance(startPosition, transform.position);
        if (distanceTraveled >= maxRange)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Stone"))
        {
            StoneHealth stone = other.GetComponent<StoneHealth>();
            if (stone != null)
            {
                stone.TakeDamage(damage, isCriticalHit);
            }
            Destroy(gameObject);
        }
    }
}