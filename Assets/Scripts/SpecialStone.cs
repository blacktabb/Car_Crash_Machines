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
    private bool isTriggered = false;

    [Header("Sandýk Ayarlarý")]
    public int goldAmount = 10; // Normal taþtan fazla olsun (Örn: Taþ 5 ise bu 50 olsun)
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
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

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
    }

    void OpenChest()
    {
        // --- DEÐÝÞÝKLÝK BURADA ---
        // Artýk LevelManager'da biriktirmiyoruz.
        // Direkt ana para yöneticisine (Cüzdana) ekliyoruz.
        // VehicleStackManager.AddMoney zaten "Save" iþlemini yapýyor.

        if (VehicleStackManager.Instance != null)
        {
            VehicleStackManager.Instance.AddMoney(goldAmount);
        }
        // -------------------------

        // Görsel efekt
        if (goldPopupPrefab != null)
        {
            GameObject popup = Instantiate(goldPopupPrefab, transform.position + Vector3.up * 2, Quaternion.identity);
            TextMeshPro textMesh = popup.GetComponent<TextMeshPro>(); // Veya TMP_Text
            if (textMesh != null)
            {
                textMesh.text = "+" + goldAmount + " G";
                textMesh.color = Color.yellow;
                textMesh.fontSize = 6;
            }
        }

        // Sandýk açýlýnca kendisini yok etsin (Görsel olarak)
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