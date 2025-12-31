using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TMP_Text textMesh;
    private Color textColor;

    [Header("Animasyon Ayarlarý")]
    public float moveSpeed = 3f;
    public float fadeSpeed = 3f;
    public float lifeTime = 1f;

    void Awake()
    {
        textMesh = GetComponent<TMP_Text>();
        if (textMesh != null)
        {
            textColor = textMesh.color;
        }
    }

    // --- DÜZELTME BURADA ---
    // Artýk 2. parametre olarak 'isCritical' alabiliyor.
    // '= false' dediðimiz için, eðer 2. deðeri göndermezsen otomatik 'false' kabul eder.
    // Yani hem Setup(10) hem de Setup(10, true) þeklinde kullanabilirsin.
    public void Setup(float damageAmount, bool isCritical = false)
    {
        if (textMesh != null)
        {
            textMesh.text = damageAmount.ToString("0.#");

            if (isCritical)
            {
                // Kritik vuruþsa yazýyý büyüt ve Kýrmýzý yap
                textMesh.fontSize *= 1.5f;
                textMesh.color = Color.red;
                textMesh.text += "!"; // Yanýna ünlem koy
            }
            else
            {
                // Normal vuruþ (Sarý veya varsayýlan renk)
                textMesh.fontSize *= 1f; // Normal boyut
                // textMesh.color = Color.yellow; // Ýstersen rengi zorla
            }

            textColor = textMesh.color; // Fade iþlemi için rengi kaydet
        }
    }

    void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        if (textMesh != null)
        {
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;

            if (textColor.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}