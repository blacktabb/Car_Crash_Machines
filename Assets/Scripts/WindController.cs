using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class WindEffectController : MonoBehaviour
{
    ParticleSystem ps;
    ParticleSystem.MainModule main;

    // Rüzgarýn normal oyun hýzýndaki (Speed = 5 iken) hýzý ne olsun?
    // 1 yaparsan normal akar, 2 yaparsan çok hýzlý akar.
    public float baseSimulationSpeed = 1.0f;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        main = ps.main;
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        // MATEMATÝK:
        // Oyun Hýzý (5) / Hedef Hýz (5) = 1 (Normal Hýz)
        // Oyun Hýzý (2.5) / Hedef Hýz (5) = 0.5 (Yarý Hýz - Slow Motion)
        // Oyun Hýzý (0) / Hedef Hýz (5) = 0 (Durmuþ)

        float ratio = 0f;

        if (GameManager.Instance.targetSpeed > 0)
        {
            ratio = GameManager.Instance.gameSpeed / GameManager.Instance.targetSpeed;
        }

        // Particle System'in kendi zaman akýþýný deðiþtiriyoruz.
        // Bu sayede hem parçacýklarýn hýzý hem de çýkýþ sýklýðý orantýlý deðiþir.
        main.simulationSpeed = ratio * baseSimulationSpeed;
    }
}