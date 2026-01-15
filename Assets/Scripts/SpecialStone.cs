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
    public int goldAmount = 10;
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
        if (VehicleStackManager.Instance != null)
        {
            VehicleStackManager.Instance.AddMoney(goldAmount);
        }

        if (goldPopupPrefab != null)
        {
            GameObject popup = Instantiate(goldPopupPrefab, transform.position + Vector3.up * 2, Quaternion.identity);
            TextMeshPro textMesh = popup.GetComponent<TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = "+" + goldAmount + " G";
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