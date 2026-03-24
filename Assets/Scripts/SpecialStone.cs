using UnityEngine;
using TMPro;

public class SpecialStone : MonoBehaviour
{
    public enum SpecialType { Dynamite, Chest }

    [Header("Tür Seçimi")]
    public SpecialType stoneType;

    [Header("Dinamit Ayarlarý")]
    public float explosionRadius = 2.0f;
    public int explosionDamage = 5;
    public GameObject explosionEffect;

    // --- YENÝ: TÝTREME AYARLARI ---
    [Header("Dinamit Titreme Ayarlarý")]
    public float shakeDuration = 0.5f;   // Ne kadar sürsün?
    public float shakeMagnitude = 0.5f;  // Ne kadar þiddetli olsun?
                                         // ------------------------------

    private bool isTriggered = false;

    [Header("Sandýk Ayarlarý")]
    [Header("Sandýk Ayarlarý")]
    public int baseGoldAmount = 10;           // Baþlangýç (1. Seviye) altýný
    public int goldIncreaseAmount = 20;       // Her eþikte kaç altýn eklensin?
    public int levelThresholdForBonus = 5;
    public GameObject goldPopupPrefab;

    public void ActivateSpecialEffect()
    {
        if (isTriggered) return;
        isTriggered = true;

        switch (stoneType)
        {
            case SpecialType.Dynamite:
                Explode();
                break;

            case SpecialType.Chest:
                OpenChest();
                break;
        }
    }

    void Explode()
    {
        // --- SES EKLE ---
        if (AudioManager.Instance != null) AudioManager.Instance.PlayExplosion();
        // ----------------

        // 1. GÖRSEL EFEKT
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        // 2. KAMERA TÝTREMESÝ (YENÝ KISIM)
        // Sahnedeki "MainCamera" etiketli kamerayý bulur ve üzerindeki scripti çalýþtýrýr.
        if (Camera.main != null)
        {
            CameraShake shaker = Camera.main.GetComponent<CameraShake>();
            if (shaker != null)
            {
                shaker.TriggerShake(shakeDuration, shakeMagnitude);
            }
        }

        // 3. HASAR VERME
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == this.gameObject) continue;

            if (hitCollider.CompareTag("Stone"))
            {
                StoneHealth stoneHealth = hitCollider.GetComponent<StoneHealth>();
                if (stoneHealth != null)
                {
                    stoneHealth.TakeDamage(explosionDamage);
                }
            }
        }

        // Dinamit iþini bitirince kendini yok etsin mi? 
        // Genelde patladýktan sonra yok olmasý gerekir.
        Destroy(gameObject);
    }

    void OpenChest()
    {
        // 1. O anki leveli al (LevelManager yoksa güvenli kalmak için 1 kabul et)
        int currentLevel = 1;
        if (LevelManager.Instance != null)
        {
            currentLevel = LevelManager.Instance.currentLevel;
        }

        // 2. Altýn miktarýný hesapla
        // Mantýk: (Level - 1) / Eþik Deðeri. 
        // Örn: Level 1-4 -> Çarpan 0 | Level 5-9 -> Çarpan 1 | Level 10-14 -> Çarpan 2
        int bonusSteps = (currentLevel - 1) / levelThresholdForBonus;
        int finalGoldAmount = baseGoldAmount + (bonusSteps * goldIncreaseAmount);

        // 3. Parayý oyuncuya ver
        if (VehicleStackManager.Instance != null)
        {
            VehicleStackManager.Instance.AddMoney(finalGoldAmount);
        }

        // 4. Görsel Popup'ý göster
        if (goldPopupPrefab != null)
        {
            GameObject popup = Instantiate(goldPopupPrefab, transform.position + Vector3.up * 2, Quaternion.identity);
            TextMeshPro textMesh = popup.GetComponent<TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = "+" + finalGoldAmount + " G";
                textMesh.color = Color.yellow;
                textMesh.fontSize = 6;
            }
        }

        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (stoneType == SpecialType.Dynamite)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}