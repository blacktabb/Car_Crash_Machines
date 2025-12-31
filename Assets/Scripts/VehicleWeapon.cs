using UnityEngine;
using TMPro;
using System.Collections;

public class VehicleWeapon : MonoBehaviour
{
    [Header("Temel Özellikler")]
    public int level = 1;
    public float baseDamage = 2f;
    public float baseFireRate = 1.0f;

    [Header("Merge Dengesi (YENÝ)")]
    // 1.5 = Her merge iþleminde %50 güçlenir (Tavsiye edilen: 1.2 ile 1.5 arasý)
    // 2.0 yaparsan eski sistemle ayný olur (%100 artýþ).
    public float damageMultiplierPerMerge = 1.20f;

    [Header("Görseller & Referanslar")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public GameObject mergeEffect;
    public Animator animator;
    public TextMeshPro levelText;

    [Header("Animasyon Ayarlarý")]
    public float mergeMoveSpeed = 15f;
    private float nextFireTime = 0f;

    // Geçici Güçlendirmeler
    [HideInInspector] public float tempDamageMultiplier = 1.0f;
    [HideInInspector] public float tempFireRateDivider = 1.0f;
    [HideInInspector] public float tempCritChanceAdd = 0f;

    void Start()
    {
        if (level <= 0) level = 1;
        UpdateVisuals();
        UpdateLevelText();
        StartCoroutine(PlaySpawnAnimation());
    }

    void Update()
    {
        HandleShooting();
    }

    // ... (UpdateVisuals ve HandleShooting Hýz kýsýmlarý ayný kalýyor) ...

    public void UpdateVisuals()
    {
        if (VehicleStackManager.Instance == null || VehicleStackManager.Instance.weaponModels == null) return;

        Transform oldModel = transform.Find("ActiveWeaponModel");
        if (oldModel != null) Destroy(oldModel.gameObject);

        int modelIndex = (level == 1) ? 0 : (int)Mathf.Log(level, 2);
        if (modelIndex >= VehicleStackManager.Instance.weaponModels.Length)
            modelIndex = VehicleStackManager.Instance.weaponModels.Length - 1;

        GameObject targetPrefab = VehicleStackManager.Instance.weaponModels[modelIndex];

        if (targetPrefab != null)
        {
            GameObject newModel = Instantiate(targetPrefab, transform);
            newModel.name = "ActiveWeaponModel";
            newModel.transform.localPosition = Vector3.zero;
            newModel.transform.localRotation = Quaternion.identity;
            newModel.transform.localScale = Vector3.one;
            animator = newModel.GetComponent<Animator>();
        }
        UpdateLevelText();
    }

    void HandleShooting()
    {
        // HIZ HESABI (Aynen Kalýyor)
        float bonusSpeed = 0f;
        if (UpgradeManager.Instance != null)
        {
            int spdLvl = PlayerPrefs.GetInt("Upg_AtkSpeed", 0);
            bonusSpeed = spdLvl * UpgradeManager.Instance.incValue_Spd;
        }

        float levelBonus = (level - 1) * 0.1f;
        float totalFireRate = baseFireRate + bonusSpeed + levelBonus;
        totalFireRate /= tempFireRateDivider;
        float fireDelay = 1.0f / totalFireRate;

        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireDelay;
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        BulletController bulletScript = bullet.GetComponent<BulletController>();

        if (bulletScript != null)
        {
            // --- YENÝ HASAR HESABI ---

            // 1. Upgrade'den gelen sabit hasarý al
            float bonusDamage = 0f;
            if (UpgradeManager.Instance != null)
            {
                int atkLvl = PlayerPrefs.GetInt("Upg_AtkPower", 0);
                bonusDamage = atkLvl * UpgradeManager.Instance.incValue_Atk;
            }

            // 2. MERGE GÜCÜNÜ HESAPLA (Üstel Artýþ)
            // Level 1 -> tier 0
            // Level 2 -> tier 1
            // Level 4 -> tier 2
            float tierIndex = Mathf.Log(level, 2); // Kaçýncý seviye olduðunu bul (0, 1, 2, 3...)

            // Çarpaný hesapla: 1.3 ^ 2 gibi
            float mergeMultiplier = Mathf.Pow(damageMultiplierPerMerge, tierIndex);

            // FORMÜL: (BazHasar * MergeÇarpaný) + UpgradeBonusu
            float finalDamage = (baseDamage * mergeMultiplier) + bonusDamage;

            // -------------------------

            // Perk Çarpaný
            finalDamage *= tempDamageMultiplier;

            // --- KRÝTÝK HESABI (Ayný Kalýyor) ---
            float critChance = 5.0f;
            float critMult = 1.5f;

            if (UpgradeManager.Instance != null)
            {
                int critRateLvl = PlayerPrefs.GetInt("Upg_CritRate", 0);
                int critDmgLvl = PlayerPrefs.GetInt("Upg_CritDmg", 0);

                critChance += (critRateLvl * UpgradeManager.Instance.incValue_CritRate);
                critMult += (critDmgLvl * UpgradeManager.Instance.incValue_CritDmg);
            }

            critChance += tempCritChanceAdd;

            bool isCritical = (Random.Range(0f, 100f) < critChance);
            if (isCritical) finalDamage *= critMult;

            // Virgüllü hasarý gönder
            bulletScript.SetDamage(finalDamage, isCritical);

            if (isCritical)
            {
                bullet.transform.localScale *= 1.3f;
                Renderer rnd = bullet.GetComponent<Renderer>();
                if (rnd != null) rnd.material.color = Color.red;
            }
        }
    }

    public IEnumerator MoveAndMerge(Vector3 targetLocalPos)
    {
        while (Vector3.Distance(transform.localPosition, targetLocalPos) > 0.05f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetLocalPos, mergeMoveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.localPosition = targetLocalPos;
        ToggleVisuals(false);
    }

    public void LevelUp()
    {
        level *= 2;
        UpdateVisuals();
        if (mergeEffect != null) Instantiate(mergeEffect, transform.position, Quaternion.identity);
    }

    void UpdateLevelText()
    {
        if (levelText != null) levelText.text = level.ToString();
    }

    void ToggleVisuals(bool state)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = state;
        if (levelText != null) levelText.enabled = state;
    }

    IEnumerator PlaySpawnAnimation()
    {
        transform.localScale = Vector3.zero;
        float timer = 0f;
        while (timer < 0.3f)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, timer / 0.3f);
            yield return null;
        }
        transform.localScale = Vector3.one;
    }
}