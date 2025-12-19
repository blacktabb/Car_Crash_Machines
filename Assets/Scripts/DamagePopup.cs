using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float disappearTimer;
    private Color textColor;

    public static void Create(Vector3 position, int damageAmount, bool isCritical)
    {
        // Kaynaklar klasöründen veya referansla prefabý bulup oluþturmak yerine
        // Genelde Singleton veya referansla çaðrýlýr ama en basit yöntem:
        // Bu fonksiyonu þimdilik scriptin içinden çaðýracaðýz.
    }

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(int damageAmount, bool isCritical)
    {
        textMesh.text = damageAmount.ToString();

        if (isCritical)
        {
            textMesh.fontSize = 8; // Kritikse büyük yaz
            textMesh.color = Color.red; // Kýrmýzý yap
        }
        else
        {
            textMesh.fontSize = 5;
            textMesh.color = Color.yellow;
        }

        textColor = textMesh.color;
        disappearTimer = 0.5f; // Yarým saniyede kaybolsun
    }

    void Update()
    {
        // Yukarý doðru süzülme
        transform.position += new Vector3(0, 5f) * Time.deltaTime;

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            // Yavaþça þeffaflaþ ve yok ol
            float disappearSpeed = 3f;
            textColor.a -= disappearSpeed * Time.deltaTime;
            textMesh.color = textColor;

            if (textColor.a < 0) Destroy(gameObject);
        }
    }
}