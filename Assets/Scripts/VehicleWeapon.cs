using UnityEngine;
using TMPro;

public class VehicleWeapon : MonoBehaviour
{
    [Header("Özellikler")]
    public int level = 1;      // Görünen Sayý (1, 2, 4, 8...)
    public int damage = 1;
    public float fireRate = 0.5f;

    // Sprite dizilimi için gizli bir sayaç tutuyoruz
    private int tierIndex = 0; // (0, 1, 2, 3...) diye gider

    [Header("Görsel Ayarlar")]
    public SpriteRenderer bodyRenderer;
    public Transform firePoint;
    public GameObject bulletPrefab;
    public TextMeshPro levelText;

    public Sprite[] levelSprites;

    private float nextFireTime;

    void Start()
    {
        // Baþlangýçta tier index'i level'e göre ayarla (Logaritma mantýðý)
        // Level 1 -> Index 0
        // Level 2 -> Index 1
        // Level 4 -> Index 2
        if (level > 0)
        {
            // Mathf.Log(level, 2) bize 2'nin kaçýncý kuvveti olduðunu verir.
            tierIndex = (int)Mathf.Log(level, 2);
        }

        UpdateVisuals();
    }

    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            bullet.GetComponent<Bullet>().damage = damage;
        }
    }

    public void LevelUp()
    {
        // --- DEÐÝÞÝKLÝK BURADA ---

        // Seviyeyi 2 ile çarpýyoruz (1->2, 2->4, 4->8...)
        level *= 2;

        // Resim sýrasýný 1 artýrýyoruz (Sýradaki resme geçmek için)
        tierIndex++;

        damage *= 2;
        fireRate *= 0.8f;

        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        // 1. Sprite Deðiþimi (tierIndex kullanýyoruz)
        if (bodyRenderer != null)
        {
            // Eðer elimizde bu seviyeye uygun resim varsa onu koy
            if (tierIndex < levelSprites.Length)
            {
                bodyRenderer.sprite = levelSprites[tierIndex];
                bodyRenderer.color = Color.white; // Rengi normale döndür
            }
            else
            {
                // Resimlerimiz bittiyse (Örn: 5 resim var ama Level 64 olduk)
                // Son resmi kullan ama rengini deðiþtirerek fark yarat
                bodyRenderer.sprite = levelSprites[levelSprites.Length - 1];

                // Her ekstra seviyede rastgele veya farklý bir ton ver
                bodyRenderer.color = Color.HSVToRGB((tierIndex * 0.1f) % 1f, 1f, 1f);
            }
        }

        // 2. LEVEL YAZISINI GÜNCELLEME
        if (levelText != null)
        {
            // Direkt sayýyý yazdýrýyoruz (2, 4, 8, 16...)
            levelText.text = level.ToString();
        }
    }
}