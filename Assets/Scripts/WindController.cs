using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class WindEffectController : MonoBehaviour
{
    [Header("Referanslar")]
    public ParticleSystem windParticle;

    [Header("Oyun Hýzý")]
    public float baseGameSpeed = 5f;

    [Header("Rüzgar Ayarlarý")]
    public float baseWindSpeed = 15f;
    public float baseEmission = 100f;
    public float baseLifetime = 0.4f;

    void Update()
    {
        float speedFactor = GameManager.Instance.gameSpeed / baseGameSpeed;

        var main = windParticle.main;
        main.startSpeed = baseWindSpeed * speedFactor;
        main.startLifetime = baseLifetime / speedFactor;

        var emission = windParticle.emission;
        emission.rateOverTime = baseEmission * speedFactor;
    }
}