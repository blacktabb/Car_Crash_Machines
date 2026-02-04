using UnityEngine;
using TMPro;
using System.Collections;

public class VehicleWeapon : MonoBehaviour
{
    [Header("Temel Özellikler")]
    public int level = 1;
    public float baseDamage = 3f;
    public float baseFireRate = 1.0f;

    [Header("Merge Dengesi")]
    public float damageMultiplierPerMerge = 1.5f;

    [Header("Görseller & Referanslar")]
    public GameObject bulletPrefab; // Varsayýlan/Fallback mermi
    // --- YENÝ: MERMÝ LÝSTESÝ (Artýk silah kendi mermilerini biliyor) ---
    public GameObject[] bulletPrefabs;
    // ------------------------------------------------------------------

    public Transform firePoint;
    public GameObject mergeEffect;
    public Animator animator;
    public TextMeshPro levelText;

    [Header("Animasyon Ayarlarý")]
    public float mergeMoveSpeed = 15f;
    private float nextFireTime = 0f;

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
        if (GameManager.Instance == null || GameManager.Instance.gameSpeed <= 0f)
            return;

        HandleShooting();
    }

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
        if (firePoint == null) return;

        // --- MERMÝ SEÇÝMÝ (YEREL LÝSTE) ---
        GameObject prefabToSpawn = bulletPrefab; // Varsayýlan

        if (bulletPrefabs != null && bulletPrefabs.Length > 0)
        {
            // Level 1 -> Index 0, Level 2 -> Index 1, Level 4 -> Index 2...
            // Log2 mantýðý ile index bulma
            int index = (level == 1) ? 0 : (int)Mathf.Log(level, 2);

            // Eðer liste sýnýrýný aþarsa sonuncu mermiyi kullan
            if (index >= bulletPrefabs.Length)
                index = bulletPrefabs.Length - 1;

            if (bulletPrefabs[index] != null)
                prefabToSpawn = bulletPrefabs[index];
        }

        if (prefabToSpawn == null) return;
        // ----------------------------------

        if (AudioManager.Instance != null) AudioManager.Instance.PlayShoot();

        GameObject bullet = Instantiate(prefabToSpawn, firePoint.position, prefabToSpawn.transform.rotation);
        BulletController bulletScript = bullet.GetComponent<BulletController>();

        if (bulletScript != null)
        {
            float bonusDamage = 0f;
            if (UpgradeManager.Instance != null)
            {
                int atkLvl = PlayerPrefs.GetInt("Upg_AtkPower", 0);
                bonusDamage = atkLvl * UpgradeManager.Instance.incValue_Atk;
            }

            float tierIndex = Mathf.Log(level, 2);
            float mergeMultiplier = Mathf.Pow(damageMultiplierPerMerge, tierIndex);
            float finalDamage = (baseDamage * mergeMultiplier) + bonusDamage;

            finalDamage *= tempDamageMultiplier;

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
        Vector3 targetScale = transform.localScale;
        transform.localScale = Vector3.zero;
        float timer = 0f;
        while (timer < 0.3f)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, timer / 0.3f);
            yield return null;
        }
        transform.localScale = targetScale;
    }
}