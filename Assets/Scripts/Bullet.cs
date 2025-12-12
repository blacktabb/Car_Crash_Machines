using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 1;

    void Update()
    {
        // Mermi sürekli saða gider
        transform.Translate(Vector2.right * speed * Time.deltaTime);

        // Ekrandan çýkarsa (Sað taraftan) yok et
        if (transform.position.x > 20f)
        {
            Destroy(gameObject);
        }
    }

    // Trigger (Tetikleyici) çarpýþma kontrolü
    void OnTriggerEnter2D(Collider2D other)
    {
        // Eðer çarptýðýmýz þey "Stone" (Taþ) ise
        if (other.CompareTag("Stone"))
        {
            // Taþýn üzerindeki StoneHealth koduna ulaþ
            StoneHealth stone = other.GetComponent<StoneHealth>();

            if (stone != null)
            {
                stone.TakeDamage(damage); // Hasar ver
            }

            // Mermiyi yok et (Görevi bitti)
            Destroy(gameObject);
        }
    }
}