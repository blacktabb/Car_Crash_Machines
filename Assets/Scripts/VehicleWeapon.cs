using UnityEngine;
using TMPro;
using System.Collections; // Coroutine için gerekli

public class VehicleWeapon : MonoBehaviour
{
    [Header("Özellikler")]
    public int level = 1;
    public int damage = 1;
    public float fireRate = 0.5f;
    private int tierIndex = 0;

    [Header("Görsel Ayarlar")]
    public SpriteRenderer bodyRenderer;
    public Transform firePoint;
    public GameObject bulletPrefab;
    public TextMeshPro levelText;
    public Sprite[] levelSprites;

    [Header("Animasyon")]
    // Inspector'dan bu eðriyi ayarlayarak "Pop" efekti vereceðiz
    public AnimationCurve spawnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float animDuration = 0.5f;

    private float nextFireTime;

    void Start()
    {
        if (level > 0) tierIndex = (int)Mathf.Log(level, 2);
        UpdateVisuals();

        // Doðma Animasyonunu Baþlat
        StartCoroutine(AnimateScale());
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
        level *= 2;
        tierIndex++;
        damage *= 2;
        fireRate *= 0.8f;

        UpdateVisuals();

        // Seviye atlayýnca da animasyon oynasýn (Görsel geri bildirim)
        StopAllCoroutines(); // Eski animasyon varsa durdur
        StartCoroutine(AnimateScale());
    }

    void UpdateVisuals()
    {
        if (bodyRenderer != null)
        {
            if (tierIndex < levelSprites.Length)
            {
                bodyRenderer.sprite = levelSprites[tierIndex];
                bodyRenderer.color = Color.white;
            }
            else
            {
                bodyRenderer.sprite = levelSprites[levelSprites.Length - 1];
                bodyRenderer.color = Color.HSVToRGB((tierIndex * 0.1f) % 1f, 1f, 1f);
            }
        }

        if (levelText != null) levelText.text = level.ToString();
    }

    // --- BÜYÜME ANÝMASYONU ---
    IEnumerator AnimateScale()
    {
        float timer = 0f;
        Vector3 targetScale = Vector3.one; // Hedef boyut (1,1,1)

        // Baþlangýçta boyutu 0 yapýyoruz
        transform.localScale = Vector3.zero;

        while (timer < 1f)
        {
            timer += Time.deltaTime / animDuration;

            // Curve kullanarak deðer alýyoruz (0'dan 1'e giderken eðriye göre davranýr)
            float scaleValue = spawnCurve.Evaluate(timer);

            transform.localScale = targetScale * scaleValue;
            yield return null;
        }

        // Garanti olsun diye döngü bitince tam boyuta eþitle
        transform.localScale = targetScale;
    }

    // ... (Mevcut kodlarýn altýna ekle)

    // Bu fonksiyonu dýþarýdan çaðýracaðýz
    public void DestroyWithAnimation()
    {
        // Collider'ý kapat ki düþerken diðerlerine çarpmasýn
        GetComponent<BoxCollider2D>().enabled = false;

        // Varsa çalýþan animasyonlarý durdur
        StopAllCoroutines();

        // Yok olma animasyonunu baþlat
        StartCoroutine(AnimateDeath());
    }

    IEnumerator AnimateDeath()
    {
        float timer = 0f;
        float duration = 0.3f; // Çok hýzlý yok olsun (0.3 saniye)
        Vector3 startScale = transform.localScale;

        while (timer < 1f)
        {
            timer += Time.deltaTime / duration;

            // Lerp ile boyutu 1'den 0'a indiriyoruz
            // Vector3.Lerp(Baþlangýç, Hedef, Zaman)
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, timer);

            yield return null;
        }

        // Animasyon bitti, artýk gerçekten yok edebiliriz
        Destroy(gameObject);
    }
}