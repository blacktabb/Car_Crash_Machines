using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    private Vector3 initialPosition;

    void Awake()
    {
        initialPosition = transform.localPosition;
    }

    // --- YENÝ FONKSÝYON: DIÞARIDAN SADECE BUNU ÇAÐIRACAÐIZ ---
    public void TriggerShake(float duration, float magnitude)
    {
        // Önce varsa eski titremeyi durdurup sýfýrlayalým
        StopAllCoroutines();
        transform.localPosition = initialPosition;

        // Þimdi yenisini baþlatalým (Kendi üzerimizde)
        StartCoroutine(DoShake(duration, magnitude));
    }

    // IEnumerator'ý 'private' yaptýk ve ismini deðiþtirdik
    private IEnumerator DoShake(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // EÐER OYUN DURDUYSA (TIMESCALE 0 ÝSE) TÝTREMEYÝ KES
            if (Time.timeScale == 0f)
            {
                transform.localPosition = initialPosition;
                yield break; // Döngüyü kýr ve çýk
            }

            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(initialPosition.x + x, initialPosition.y + y, initialPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = initialPosition;
    }

    public void StopShake()
    {
        StopAllCoroutines();
        transform.localPosition = initialPosition;
    }
}