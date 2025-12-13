using UnityEngine;

public class VehicleWeapon : MonoBehaviour
{
    [Header("Özellikler")]
    public int level = 1;
    public int damage = 1;
    public float fireRate = 0.5f;

    [Header("Görsel Ayarlar")]
    public SpriteRenderer bodyRenderer; // Aracýn kendi resmi
    public Transform firePoint;         // Merminin çýkacaðý yer
    public GameObject bulletPrefab;

    // Seviye atlayýnca deðiþecek araç resimleri (Opsiyonel)
    // Inspector'dan Level 1, Level 2, Level 3 arabalarýný buraya sürükleyebilirsin.
    public Sprite[] levelSprites;

    private float nextFireTime;

    void Start()
    {
        // Baþlangýç görselini ayarla
        //UpdateVisuals();
    }

    void Update()
    {
        // Sürekli ateþ et
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
            // Mermiye hasar gücünü aktar
            bullet.GetComponent<Bullet>().damage = damage;
        }
    }

    // --- UPGRADE FONKSÝYONU ---
    public void LevelUp()
    {
        level++;
        damage *= 2;        // Güç 2 katýna
        fireRate *= 0.8f;   // Ateþ hýzý %20 artar (Süre azalýr)

        //UpdateVisuals();

        // Havalý bir efekt (Büyüyüp küçülme animasyonu gibi)
        transform.localScale = Vector3.one * 1.2f; // Biraz büyüsün
    }

    //void UpdateVisuals()
    //{
    //    // Eðer elimizde bu seviye için özel bir resim varsa onu koy
    //    if (levelSprites.Length >= level && bodyRenderer != null)
    //    {
    //        bodyRenderer.sprite = levelSprites[level - 1];
    //    }
    //    else
    //    {
    //        // Resim yoksa renk deðiþtirerek belli et
    //        bodyRenderer.color = Color.Lerp(Color.white, Color.red, level * 0.2f);
    //    }
    //}
}