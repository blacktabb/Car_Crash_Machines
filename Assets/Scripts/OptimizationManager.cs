using UnityEngine;
using System.Collections.Generic;

public class OptimizationManager : MonoBehaviour
{
    public static OptimizationManager Instance;

    [Header("Ayarlar")]
    public Transform cameraTransform;   // Kamerayý buraya sürükle
    public float activationDistance = 25f; // Kamera önünde kaç metre aktif olsun?
    public float destroyDistance = 15f;    // Kamera arkasýnda kaç metre sonra yok olsun?
    public float checkInterval = 0.2f;     // Saniyede kaç kez kontrol etsin? (0.2 = saniyede 5 kez)

    // Optimizasyon için verileri sakladýðýmýz basit bir sýnýf
    public class OptimizedStone
    {
        public GameObject obj;
        public Transform trans;
        public Renderer[] renderers;
        public Collider[] colliders;
        public bool isActive;
    }

    private List<OptimizedStone> allStones = new List<OptimizedStone>();
    private float timer;

    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
            Debug.LogWarning("OptimizationManager: Kamera Transform'u atanmadý, otomatik olarak ana kamerayý buldu.");
        }
    }

    void Awake()
    {
       
        if (Instance == null) Instance = this;
        //else Destroy(this);

        cameraTransform = Camera.main.transform;
    }

    // --- LevelGenerator BU FONKSÝYONU ÇAÐIRACAK ---
    public void RegisterStone(GameObject stoneObj)
    {
        OptimizedStone data = new OptimizedStone();
        data.obj = stoneObj;
        data.trans = stoneObj.transform;

        // Bileþenleri önbelleðe al (Cache) - Bu sayede Update'de GetComponent yapmayýz
        data.renderers = stoneObj.GetComponentsInChildren<Renderer>();
        data.colliders = stoneObj.GetComponentsInChildren<Collider>();
        data.isActive = true; // Baþta aktif

        allStones.Add(data);
    }

    void Update()
    {
        // Her karede deðil, belirli aralýklarla çalýþtýr (Performans Tasarrufu)
        timer += Time.deltaTime;
        if (timer < checkInterval) return;
        timer = 0f;
        
        if (cameraTransform == null) { cameraTransform = Camera.main.transform; } // Kamera hareket ediyor olabilir, her kontrol güncellemesi yapalým
        float camX = cameraTransform.position.x;

        // Tersten döngü kuruyoruz ki listeden silme yaparsak sorun olmasýn
        for (int i = allStones.Count - 1; i >= 0; i--)
        {
            OptimizedStone stone = allStones[i];

            // Güvenlik: Taþ yok edildiyse listeden sil
            if (stone.obj == null)
            {
                allStones.RemoveAt(i);
                continue;
            }

            float stoneX = stone.trans.position.x;
            float distanceToCam = stoneX - camX;

            // 1. KAMERA ARKASINDA KALDIYSA (Çok uzaklaþtýysa)
            // (Ekrandan çýktý ve geride kaldý, artýk yok edebiliriz)
            if (distanceToCam < -destroyDistance)
            {
                Destroy(stone.obj);
                allStones.RemoveAt(i);
                continue;
            }

            // 2. GÖRÜÞ ALANI KONTROLÜ
            // Kamera önünde 'activationDistance' içindeyse VEYA kamera arkasýnda çok az mesafedeyse
            bool shouldBeActive = (distanceToCam <= activationDistance && distanceToCam > -10f);

            if (stone.isActive != shouldBeActive)
            {
                ToggleStone(stone, shouldBeActive);
            }
        }
    }

    // Görseli ve Fiziði Kapat/Aç (Ama objeyi kapatma, hareket sürsün)
    // Görseli Kapat ama FÝZÝÐÝ AÇIK TUT (Kritik Düzeltme)
    void ToggleStone(OptimizedStone stone, bool state)
    {
        stone.isActive = state;

        // 1. GÖRSELLÝK (Performansý asýl yiyen budur, bunu kapatýyoruz)
        foreach (var r in stone.renderers)
        {
            r.enabled = state;
        }

        // 2. FÝZÝK (COLLIDER)
        // BURAYI ÝPTAL EDÝYORUZ / YORUM SATIRI YAPIYORUZ
        // Collider'lar hep açýk kalsýn ki taþlar birbirini hissedebilsin.
        // Kinematik colliderlar iþlemciyi çok yormaz.

        /* foreach (var c in stone.colliders) 
        {
            c.enabled = state; 
        }
        */
    }

    // --- EKSÝK OLAN FONKSÝYON: TÜM TAÞLARI TEMÝZLE ---
    // Level deðiþirken eski taþlarý silmek için kullanýyoruz.
    public void ClearAllStones()
    {
        // 1. Sahnedeki tüm taþlarý yok et
        foreach (var stone in allStones)
        {
            if (stone.obj != null)
            {
                Destroy(stone.obj);
            }
        }

        // 2. Listeyi boþalt
        allStones.Clear();

        Debug.Log("OptimizationManager: Tüm taþlar temizlendi.");
    }
}