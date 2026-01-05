using UnityEngine;

public class FinishLineController : MonoBehaviour
{
    [Header("Hýz Kalibrasyonu")]
    // Eðer çizgi taþlardan hýzlý geliyorsa bunu 0.9 veya 0.8 yap.
    // Eðer yavaþ geliyorsa 1.1 yap.
    public float speedMultiplier = 1.0f;

    private bool isTriggered = false;

    void Update()
    {
        if (GameManager.Instance == null) return;

        // 1. HAREKET MANTIÐI
        float speed = GameManager.Instance.gameSpeed;

        // 2. HAREKET UYGULAMA
        // speedMultiplier ile çarpýyoruz ki ince ayar yapabilelim.
        // Space.World ekledik, böylece obje dönse bile dünya koordinatlarýna göre dümdüz sola gider.
        transform.Translate(Vector3.left * speed * speedMultiplier * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        if (other.CompareTag("Player") || other.GetComponentInParent<VehicleStackManager>() != null)
        {
            isTriggered = true;
            Debug.Log("FÝNÝÞ ÇÝZGÝSÝ YAKALANDI!");

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.ForceFinishLevel();
            }
        }
    }
}