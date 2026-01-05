using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    // Oyunun genel akýþý SOLA (Eksi X) olduðu için varsayýlan ayarlarý buna göre kilitledim.
    // Artýk ayar yapmana gerek yok, direkt çalýþacak.

    [Header("Otomatik Ayarlar")]
    public float calibration = 1.0f; // Eðer gözüne hala kayýk gelirse bunu 0.99 veya 1.01 yapabilirsin.

    private Renderer meshRenderer;
    private Material mat;
    private float currentOffset = 0f;
    private float sizeRatio; // Dünya koordinatýný Texture koordinatýna çeviren oran

    void Start()
    {
        meshRenderer = GetComponent<Renderer>();
        mat = meshRenderer.material;
        RecalculateRatio();
    }

    void OnValidate()
    {
        if (meshRenderer != null) RecalculateRatio();
    }

    void RecalculateRatio()
    {
        if (meshRenderer == null) meshRenderer = GetComponent<Renderer>();
        if (mat == null) mat = meshRenderer.material;

        // Oyun X ekseninde aktýðý için:
        // Zeminin gerçek geniþliði (Mesh Bounds X)
        float objectWidth = meshRenderer.bounds.size.x;

        // Texture'ýn kaç kere tekrar ettiði (Tiling X)
        float textureTilingX = mat.mainTextureScale.x;

        // ORAN: Texture tekrarý / Gerçek Geniþlik
        // Örnek: Zemin 100 metre, Texture 1 kere tekrar ediyorsa -> 1 metre gitmek 0.01 offset demektir.
        if (objectWidth > 0)
        {
            sizeRatio = textureTilingX / objectWidth;
        }
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        float currentSpeed = GameManager.Instance.gameSpeed;

        // Formül: Hýz * Zaman * Oran * Kalibrasyon
        // Taþlar Sola (Eksi X) gidiyor. Texture'ýn da saða akmasý lazým ki biz sola gidiyormuþuz gibi olsun.
        // Bu yüzden pozitif ekliyoruz.
        float moveAmount = currentSpeed * sizeRatio * calibration * Time.deltaTime;

        currentOffset += -moveAmount;
        currentOffset = currentOffset % 1f;

        // Sadece X ekseni offsetini güncelle
        float currentY = mat.mainTextureOffset.y;
        mat.mainTextureOffset = new Vector2(currentOffset, currentY);
    }
}