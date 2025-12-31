using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("Mermi Ayarlarý")]
    public float speed = 10f;
    public float maxRange = 15f; // MENZÝL AYARI: Mermi kaç metre gidecek?

    private Vector3 startPosition; // Merminin doðduðu yer
    private float damage;
    private bool isCriticalHit = false;

    public GameObject damagePopupPrefab;

    void Start()
    {
        // Mermi oluþtuðu an, nerede doðduðunu kaydediyoruz
        startPosition = transform.position;
    }

    public void SetDamage(float amount, bool isCritical)
    {
        damage = amount;
        isCriticalHit = isCritical;
    }

    void Update()
    {
        // 1. HAREKET (Saða Doðru)
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // 2. MENZÝL KONTROLÜ (YENÝ SÝSTEM)
        // Þu anki konum ile Baþlangýç konumu arasýndaki mesafeyi ölçüyoruz.
        float distanceTraveled = Vector3.Distance(startPosition, transform.position);

        // Eðer mesafe, belirlediðimiz menzili geçtiyse mermiyi yok et
        if (distanceTraveled >= maxRange)
        {
            Destroy(gameObject);
        }
    }

    // 3. ÇARPIÞMA (Aynen Kalýyor)
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Stone"))
        {
            StoneHealth stone = other.GetComponent<StoneHealth>();
            if (stone != null)
            {
                // Sadece hasar verisini gönderiyoruz (float olarak)
                // Popup çýkarma iþini artýk StoneHealth.cs içindeki TakeDamage fonksiyonu yapýyor.
                stone.TakeDamage(damage, isCriticalHit);
            }

            // Mermiyi yok et
            Destroy(gameObject);
        }
    }
}