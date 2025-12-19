using UnityEngine;
using TMPro;
using System.Collections;

public class VehicleWeapon : MonoBehaviour
{
    [Header("Temel Özellikler")]
    public int level = 1;
    public float baseDamage = 2f;
    public float baseFireRate = 1.0f;

    [Header("Görseller & Referanslar")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public GameObject mergeEffect;
    public Animator animator;
    public TextMeshPro levelText;

    [Header("Animasyon Ayarlarý")]
    public float mergeMoveSpeed = 15f;

    private float nextFireTime = 0f;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        UpdateLevelText();

        // --- YENÝ: DOÐMA ANÝMASYONUNU BAÞLAT ---
        StartCoroutine(PlaySpawnAnimation());
        // ---------------------------------------
    }

    void Update()
    {
        HandleShooting();
    }

    // --- YENÝ EKLENEN POP-UP ANÝMASYONU ---
    IEnumerator PlaySpawnAnimation()
    {
        // 1. Baþlangýçta görünmez (boyut 0) yap
        transform.localScale = Vector3.zero;

        float timer = 0f;
        float duration = 0.3f; // Animasyon 0.3 saniye sürsün

        // 2. Elastik Büyüme (Overshoot)
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // "BackOut" Easing Fonksiyonu (Matematiksel Zýplama Efekti)
            // Bu formül 0'dan baþlar, 1'i biraz geçer (þiþer) ve 1'e geri döner.
            float c1 = 1.70158f;
            float c3 = c1 + 1;
            float scale = 1 + c3 * Mathf.Pow(progress - 1, 3) + c1 * Mathf.Pow(progress - 1, 2);

            transform.localScale = Vector3.one * scale;
            yield return null;
        }

        // 3. Garanti olsun diye en son 1'e sabitle
        transform.localScale = Vector3.one;
    }
    // ----------------------------------------

    void HandleShooting()
    {
        int spdUpgradeLevel = PlayerPrefs.GetInt("Upg_AtkSpeed", 0);
        float reductionMultiplier = 1.0f - (spdUpgradeLevel * 0.02f) - ((level - 1) * 0.05f);
        if (reductionMultiplier < 0.1f) reductionMultiplier = 0.1f;
        float finalFireRate = baseFireRate * reductionMultiplier;

        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + finalFireRate;
        }
    }

    void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            BulletController bulletScript = bullet.GetComponent<BulletController>();

            if (bulletScript != null)
            {
                int atkUpgradeLevel = PlayerPrefs.GetInt("Upg_AtkPower", 0);
                float rawDamage = (baseDamage * level) + (atkUpgradeLevel * 5);

                int critRateLvl = PlayerPrefs.GetInt("Upg_CritRate", 0);
                int critDmgLvl = PlayerPrefs.GetInt("Upg_CritDmg", 0);
                float critChance = 5.0f + (critRateLvl * 2.0f);
                float critMultiplier = 1.5f + (critDmgLvl * 0.1f);
                bool isCritical = (Random.Range(0f, 100f) < critChance);

                if (isCritical)
                {
                    rawDamage *= critMultiplier;
                    bullet.transform.localScale *= 1.3f;
                    SpriteRenderer sr = bullet.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = Color.red;
                }

                bulletScript.SetDamage(Mathf.RoundToInt(rawDamage), isCritical);
            }
        }
    }

    // --- BÝRLEÞME VE HAREKET FONKSÝYONLARI ---

    public IEnumerator MoveAndMerge(Vector3 targetLocalPos)
    {
        while (Vector3.Distance(transform.localPosition, targetLocalPos) > 0.05f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetLocalPos, mergeMoveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.localPosition = targetLocalPos;

        GetComponent<SpriteRenderer>().enabled = false;
        if (levelText != null) levelText.enabled = false;
    }

    public IEnumerator SmoothMove(Vector3 targetLocalPos)
    {
        while (Vector3.Distance(transform.localPosition, targetLocalPos) > 0.05f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetLocalPos, mergeMoveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.localPosition = targetLocalPos;
    }

    public void LevelUp()
    {
        level *= 2;
        UpdateLevelText();

        if (mergeEffect != null) Instantiate(mergeEffect, transform.position, Quaternion.identity);
        if (animator != null) animator.SetTrigger("LevelUp");

        // --- ÝSTEÐE BAÐLI: LEVEL ATLAYINCA DA HAFÝF ZIPLASIN ---
        // StartCoroutine(PlaySpawnAnimation()); 
        // Bunu açarsan level atladýðýnda da ayný pop-up efektini yapar.
    }

    void UpdateLevelText()
    {
        if (levelText != null) levelText.text = level.ToString();
    }

    public void DestroyWithAnimation()
    {
        if (animator != null) animator.SetTrigger("MergeDestroy");
        else
        {
            GetComponent<SpriteRenderer>().enabled = false;
            if (levelText != null) levelText.enabled = false;
        }
    }
}