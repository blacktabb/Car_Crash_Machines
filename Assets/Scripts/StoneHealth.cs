using UnityEngine;
using TMPro;

public class StoneHealth : MonoBehaviour
{
    [Header("Ayarlar")]
    public int currentHealth;

    // --- YENÝ DEÐÝÞKEN ---
    private int goldValue; // Taþýn ödül deðeri (Baþlangýç canýna eþit olacak)
    // ---------------------

    public TextMeshPro textMesh;
    public GameObject deathEffect;

    // Spawner bu fonksiyonu çaðýrýp taþa can veriyor
    public void SetHealth(int amount)
    {
        currentHealth = amount;

        // --- EKLENEN KISIM ---
        // Baþlangýç caný neyse, altýn deðeri de o olsun.
        goldValue = amount;
        // ---------------------

        UpdateText();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        UpdateText();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateText()
    {
        if (textMesh != null)
            textMesh.text = currentHealth.ToString();
    }

    void Die()
    {
        // Level Ýlerlemesi
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.AddProgress(1);
        }

        // --- PARA KAZANMA KISMI GÜNCELLENDÝ ---
        VehicleStackManager manager = Object.FindFirstObjectByType<VehicleStackManager>();
        if (manager != null)
        {
            // Artýk sabit 10 deðil, taþýn 'goldValue' deðeri kadar para veriyoruz.
            manager.AddMoney(goldValue);
        }
        // ---------------------------------------

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}