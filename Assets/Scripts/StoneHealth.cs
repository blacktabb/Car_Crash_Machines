using UnityEngine;
// Eðer UI veya TextMeshPro kullanacaksan kütüphaneyi ekle
using TMPro;

public class StoneHealth : MonoBehaviour
{
    public int health = 3; // Taþýn kaç vuruþta kýrýlacaðý
    public GameObject deathEffect; // Taþ kýrýlýnca çýkacak efekt (Opsiyonel)

    // Taþýn üzerine canýný yazmak istersen (TextMeshPro bileþeni varsa)
    private TextMeshPro textDisplay;

    void Start()
    {
        // Taþýn içinde TextMeshPro varsa onu bul
        textDisplay = GetComponentInChildren<TextMeshPro>();
        UpdateText();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        UpdateText();

        if (health <= 0)
        {
            Die();
        }
    }

    void UpdateText()
    {
        if (textDisplay != null)
            textDisplay.text = health.ToString();
    }

    void Die()
    {
        // --- DÜZELTÝLEN KISIM BURASI ---
        // Unity 6 için güncel arama kodu:
        VehicleStackManager manager = Object.FindFirstObjectByType<VehicleStackManager>();

        // Eðer yöneticiyi bulduysa parayý ekle
        if (manager != null)
        {
            manager.AddMoney(10); // Her taþ 10 dolar kazandýrsýn
        }

        // Efekt ve Yok etme
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    // Bu fonksiyonu StoneHealth class'ýnýn içine ekle
    public void SetHealth(int maxHealth)
    {
        health = maxHealth;
        UpdateText(); // Can deðiþtiði an üzerindeki yazýyý da güncelle
    }
}