using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class StoneMover : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate() // Fizik iþlemleri her zaman FixedUpdate'te yapýlýr
    {
        // X ekseninde sürekli sola git (-moveSpeed)
        // Y ekseninde ise kendi hýzý neyse onu koru (Düþüyorsa düþmeye devam etsin: rb.velocity.y)
        rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);

        // NOT: Unity 6 öncesi sürümlerde 'linearVelocity' yerine 'velocity' kullanýlýr.
        // Eðer hata alýrsan 'rb.velocity' yaz.
    }

    void Update()
    {
        // Ekrandan çýktýysa yok et (Performans)
        if (transform.position.x < -15f)
        {
            Destroy(gameObject);
        }
    }
}
