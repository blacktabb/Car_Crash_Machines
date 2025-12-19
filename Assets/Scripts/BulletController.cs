using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed = 10f;
    private int damage;
    private bool isCriticalHit = false; // Kritik vuruþ bilgisini tutmak için

    // --- YENÝ ---
    public GameObject damagePopupPrefab; // Editörden sürükleyeceksin
    // ------------

    public void SetDamage(int amount, bool isCritical) // Kritik bilgisini de alalým
    {
        damage = amount;
        isCriticalHit = isCritical;
    }

    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
        if (transform.position.x > 20f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Stone"))
        {
            StoneHealth stone = other.GetComponent<StoneHealth>();
            if (stone != null)
            {
                stone.TakeDamage(damage);

                // --- POPUP OLUÞTURMA ---
                if (damagePopupPrefab != null)
                {
                    // Taþýn biraz üzerinde oluþtur
                    Vector3 popupPos = other.transform.position + new Vector3(0, 0.5f, 0);

                    GameObject popupObj = Instantiate(damagePopupPrefab, popupPos, Quaternion.identity);
                    DamagePopup popupScript = popupObj.GetComponent<DamagePopup>();

                    if (popupScript != null)
                    {
                        popupScript.Setup(damage, isCriticalHit);
                    }
                }
                // -----------------------
            }
            Destroy(gameObject);
        }
    }
}