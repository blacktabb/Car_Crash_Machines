using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Header("Ayarlar")]
    // Bu hýzý taþlarýn geliþ hýzýyla ayný yaparsan gerçekçi durur.
    // Taþlar aþaðý (sana doðru) geliyorsa, yolun da aþaðý akmasý lazým.
    public float scrollSpeed = 0.5f;

    private Renderer meshRenderer;
    private Material mat;

    void Start()
    {
        meshRenderer = GetComponent<Renderer>();
        mat = meshRenderer.material;
    }

    void Update()
    {
        // LevelManager'dan oyun hýzýný çekip çarpan olarak kullanýyoruz
        // Bu sayede oyun hýzlandýkça yol da hýzlanýr.
        float gameSpeed = 1.0f;
        if (LevelManager.Instance != null)
        {
            // Eðer LevelManager'da public bir speed deðiþkenin varsa onu kullan
            // Örnek: gameSpeed = LevelManager.Instance.currentSpeed;
            // Þimdilik manuel bir çarpan koyuyorum:
            gameSpeed = 1.0f + (LevelManager.Instance.currentLevel * 0.1f);
        }

        float offset = Time.time * scrollSpeed * gameSpeed;
        mat.mainTextureOffset = new Vector2(-offset, 0);
    }
}