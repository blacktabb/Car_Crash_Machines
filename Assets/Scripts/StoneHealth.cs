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
        // Efekt varsa oluþtur
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Taþý yok et
        Destroy(gameObject);
    }
}