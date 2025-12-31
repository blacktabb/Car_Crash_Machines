using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoneHealth : MonoBehaviour
{
    [Header("Can Ayarlarý")]
    public float maxHealth = 10;
    public float currentHealth = 10;

    [Header("Ödül Ayarlarý")]
    public int goldValue = 1;

    [Header("Görsel Ayarlar")]
    public GameObject damagePopupPrefab; // Hasar yazýsý prefabý
    public GameObject goldPopupPrefab;   // Altýn yazýsý prefabý
    public GameObject deathEffect;

    [Header("Vuruþ Hissiyatý")]
    public Renderer stoneRenderer;
    public Color hitColor = new Color(1f, 0.8f, 0.8f);
    private Color originalColor;
    private Vector3 originalScale;
    public float recoverySpeed = 20f;
    private Slider healthSlider;
    private bool isDamaged = false;

    [Header("Shader Ayarlarý")]
    // Shader Graph'da Reference kýsmýna yazdýðýmýz isim (Genelde _ ile baþlar)
    private string crackProperty = "_CrackAmount";
    private Material myMaterial;

    // --- KÝLÝT MEKANÝZMASI ---
    private bool isDead = false;

    void Start()
    {
        originalScale = transform.localScale;
        if (originalScale == Vector3.zero) originalScale = Vector3.one;
        currentHealth = maxHealth;

        // Shader materyalini al
        if (stoneRenderer != null)
        {
            myMaterial = stoneRenderer.material;
            UpdateCrackEffect(); // Baþlangýçta sýfýrla
        }
    }

    public void SetHealth(int amount)
    {
        maxHealth = amount;
        currentHealth = amount;
        UpdateCrackEffect(); // Can deðiþince güncelle
    }

    // --- HASAR ALMA FONKSÝYONU ---
    // isCritical opsiyonel parametresi eklendi. Mermiden true gelirse kýrmýzý yazar.
    public void TakeDamage(float damage, bool isCritical = false)
    {
        if (isDead) return;

        // 1. GÖRSEL EFEKTLER (Büyüme & Renk)
        transform.localScale = originalScale * 1.2f;
        if (stoneRenderer != null) stoneRenderer.material.color = hitColor;

        if (!isDamaged)
        {
            isDamaged = true;
            if (healthSlider == null) healthSlider = GetComponentInChildren<Slider>();
            if (healthSlider != null) healthSlider.gameObject.SetActive(true);
        }

        // 2. HASAR POPUP'INI OLUÞTUR (EKSÝK OLAN KISIM BUYDU)
        if (damagePopupPrefab != null)
        {
            ShowDamagePopup(damage, isCritical);
        }

        // 3. CAN AZALTMA
        currentHealth -= damage;

        UpdateCrackEffect(); // Can deðiþince güncelle

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Update()
    {
        if (transform.localScale.x > originalScale.x)
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * recoverySpeed);

        if (stoneRenderer != null && stoneRenderer.material.color != Color.white)
            stoneRenderer.material.color = Color.Lerp(stoneRenderer.material.color, Color.white, Time.deltaTime * recoverySpeed);
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (LevelManager.Instance != null) LevelManager.Instance.AddProgress(1);

        VehicleStackManager manager = VehicleStackManager.Instance;
        SpecialStone special = GetComponent<SpecialStone>();

        // SANDIK VEYA DÝNAMÝT ÝSE...
        if (special != null && (special.stoneType == SpecialStone.SpecialType.Chest || special.stoneType == SpecialStone.SpecialType.Dynamite))
        {
            special.ActivateSpecialEffect();
            // Ýstersen upgrade bonusunu sandýklara da ekleyebilirsin ama genelde ayrý tutulur.
        }
        // NORMAL TAÞ ÝSE...
        else
        {
            if (manager != null)
            {
                // --- DEÐÝÞÝKLÝK BURADA: BONUS HESAPLAMA ---
                int bonusGold = 0;

                // Eðer UpgradeManager varsa bonusu hesapla
                if (UpgradeManager.Instance != null)
                {
                    int goldLvl = PlayerPrefs.GetInt("Upg_GoldGain", 0);
                    // (Upgrade Leveli * Sabit Artýþ Deðeri)
                    bonusGold = goldLvl * UpgradeManager.Instance.incValue_Gold;
                }

                // Normal Deðer + Bonus Deðer
                int finalGold = goldValue + bonusGold;

                manager.AddMoney(finalGold);
                ShowGoldPopup(finalGold); // Hesaplanan son deðeri gönder
                // ------------------------------------------
            }
        }

        if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    // Hasar Yazýsý Çýkarma
    void ShowDamagePopup(float amount, bool isCritical)
    {
        // Yazýyý taþýn biraz üzerinde oluþtur
        GameObject popup = Instantiate(damagePopupPrefab, transform.position + Vector3.up * 1.0f, Quaternion.identity);

        // DamagePopup scriptine ulaþ ve ayarla
        DamagePopup script = popup.GetComponent<DamagePopup>();
        if (script != null)
        {
            script.Setup(amount, isCritical);
        }
    }

    // Altýn Yazýsý Çýkarma
    void ShowGoldPopup(int amount)
    {
        if (goldPopupPrefab != null)
        {
            GameObject popup = Instantiate(goldPopupPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
            TextMeshPro text = popup.GetComponent<TextMeshPro>();
            if (text != null)
            {
                text.text = "+" + amount + " G"; // Hesaplanan son tutarý yaz
                text.color = Color.yellow;
                text.fontSize = 5;
            }
        }
    }

    // Çatlak Efektini Güncelleme
    void UpdateCrackEffect() 
    {
        if (myMaterial == null) return;

        // Oran Hesapla: 
        // Can Full (10/10) -> Oran 1.0 -> Çatlak 0.0 olmalý
        // Can Yarým (5/10) -> Oran 0.5 -> Çatlak 0.5 olmalý
        // Can Bitti (0/10) -> Oran 0.0 -> Çatlak 1.0 olmalý

        float healthRatio = (float)currentHealth / maxHealth;
        float crackValue = 1f - healthRatio; // Tersi lazým çünkü

        // Deðeri 0 ile 1 arasýna sýkýþtýr (Garanti olsun)
        crackValue = Mathf.Clamp01(crackValue);

        // Shader'a gönder
        myMaterial.SetFloat(crackProperty, crackValue);
    }
}