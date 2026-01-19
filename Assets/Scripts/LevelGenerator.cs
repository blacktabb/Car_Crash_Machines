using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [System.Serializable]
    public struct LevelTheme
    {
        public string themeName;

        [Header("Çevre Materyalleri (YENÝ)")]
        public Material groundMaterial;      // Ana zemin (Yol)
        public Material groundSideMaterial;  // Zeminin yan yüzeyi (Varsa)
        public Material backgroundMaterial;  // Arkadaki büyük plane (Texture deðiþecek dediðin kýsým)
        public Material sideWallMaterial;    // Yan taraftaki duvar/plane

        [Header("Özel Bölge Taþlarý")]
        public GameObject[] topStonePrefabs;

        [Header("Standart Taþlar")]
        public GameObject[] smallStones;
        public GameObject[] bigStones;
        public GameObject[] rareStones;

        [Header("Gökyüzü")]
        public Material skyboxMaterial;
    }

    [Header("Tema Sistemi")]
    public LevelTheme[] themes;

    [Header("Sahne Objeleri (Referanslar)")]
    // Unity Inspector'dan sahnendeki objeleri buraya sürükleyeceksin
    public Renderer groundRenderer;      // Yürüdüðümüz yolun Renderer'ý
    public Renderer groundSideRenderer;  // Yolun yanýndaki süsün Renderer'ý
    public Renderer backgroundRenderer;  // Arkaplan Plane'inin Renderer'ý
    public Renderer sideWallRenderer;    // Yan duvarýn Renderer'ý
    public Light directionalLight; // Ana ýþýk kaynaðý (Güneþ)

    [Header("Görsel Ayarlar")]
    public int topLayerCount = 1;

    [Header("Dinamik Yükseklik Ayarlarý")]
    public int startMaxHeight = 3;
    public int absoluteMaxHeight = 12;
    public int increaseEveryXLevel = 3;
    public int heightVariation = 2;

    [Header("Dinamik Uzunluk Ayarlarý")]
    public int baseLevelLength = 50;       // Level 1 uzunluðu
    public int lengthIncreasePerLevel = 5; // Level baþý artýþ
    public int maxLevelLength = 250;       // Max sýnýr

    [Header("Debug (Sadece Ýzleme Ýçin)")]
    public int currentCalculatedLength;

    public static LevelGenerator Instance;
    public int CurrentLevelMaxHeight { get; private set; }

    [Header("Grid Ayarlarý")]
    public float cellSize = 1f;
    public float startXOffset = 10f;
    public float scrollSpeed = 2f;

    [Header("Þans")]
    [Range(0f, 1f)] public float bigStoneChance = 0.3f;
    [Range(0f, 1f)] public float rareStoneChance = 0.05f;

    [Header("Zorluk Ayarlarý")]
    public int baseHealth = 2;
    public float levelMultiplier = 1.5f;
    public int bigStoneHealthMult = 3;

    [Header("Bitiþ Ayarlarý")]
    public GameObject finishLinePrefab;
    public float finishLineOffset = 5f;

    private bool[,] gridMap;

    void Awake()
    {
        if (Instance == null) Instance = this;

        CalculateCurrentLevelMaxHeight();
        CalculateLevelLength();
    }

    void Start()
    {
        // Grid haritasýný hesaplanan uzunluða göre oluþturuyoruz
        gridMap = new bool[currentCalculatedLength, absoluteMaxHeight + 5];

        GenerateGridAndSpawn();
    }

    // --- UZUNLUK HESABI ---
    void CalculateLevelLength()
    {
        int currentLevel = (LevelManager.Instance != null) ? LevelManager.Instance.currentLevel : 1;

        int calculated = baseLevelLength + ((currentLevel - 1) * lengthIncreasePerLevel);
        currentCalculatedLength = Mathf.Min(calculated, maxLevelLength);

        Debug.Log($"LEVEL: {currentLevel} | UZUNLUK: {currentCalculatedLength} (Max: {maxLevelLength})");
    }
    // ----------------------

    // --- YENÝ EKLENEN FONKSÝYON: ÇEVREYÝ GÜNCELLE ---
    void UpdateEnvironmentVisuals(LevelTheme theme)
    {
        // 1. Zemin Materyali
        if (groundRenderer != null && theme.groundMaterial != null)
            groundRenderer.material = theme.groundMaterial;

        // 2. Zemin Yan Yüzeyi
        if (groundSideRenderer != null && theme.groundSideMaterial != null)
            groundSideRenderer.material = theme.groundSideMaterial;

        // 3. Arkaplan
        if (backgroundRenderer != null && theme.backgroundMaterial != null)
            backgroundRenderer.material = theme.backgroundMaterial;

        // 4. Yan Duvar
        if (sideWallRenderer != null && theme.sideWallMaterial != null)
            sideWallRenderer.material = theme.sideWallMaterial;

        // 5. Skybox
        if (theme.skyboxMaterial != null)
        {
            RenderSettings.skybox = theme.skyboxMaterial;
            DynamicGI.UpdateEnvironment(); // Iþýklandýrmayý skybox'a göre güncelle
        }

        // 6. Iþýk Rengi (Opsiyonel: Tema bazlý ýþýk rengi deðiþimi)
        if (directionalLight != null)
        {
            // 0 ile 2PI arasýnda bir açý (Döngü için)
            // currentLevel 1 iken 0 olsun istiyoruz.
            float cycle = ((LevelManager.Instance.currentLevel - 1) % 20) / 20f * Mathf.PI * 2;

            // Sinüs -1 ile 1 arasý deðer verir. Bunu 0.3 (Gece) ile 1.2 (Öðlen) arasýna haritalayalým.
            // (Sinüs + 1) / 2 -> 0 ile 1 arasý deðer verir.
            float intensity = 0.3f + (((Mathf.Cos(cycle) + 1f) / 2f) * 0.9f);

            // Cosinus kullandýk çünkü level 1'de (cycle 0) en yüksek (gündüz) baþlasýn.

            directionalLight.intensity = intensity;

            // Ýstersen renk tonunu da deðiþtirebilirsin (Opsiyonel)
            // directionalLight.color = Color.Lerp(Color.blue, Color.white, intensity);
        }
    }
    // -----------------------------------------------

    public void GenerateNewLevel()
    {
        if (OptimizationManager.Instance != null)
        {
            OptimizationManager.Instance.ClearAllStones();
        }

        CalculateLevelLength(); // Yeni uzunluðu hesapla
        gridMap = new bool[currentCalculatedLength, absoluteMaxHeight + 5];
        CalculateCurrentLevelMaxHeight();
        GenerateGridAndSpawn();
    }

    void GenerateGridAndSpawn()
    {
        int totalStonesSpawned = 0;
        int currentLevel = (LevelManager.Instance != null) ? LevelManager.Instance.currentLevel : 1;
        int currentMaxHeight = CurrentLevelMaxHeight;

        int currentMinHeight = Mathf.Max(1, currentMaxHeight - heightVariation);

        int themeIndex = (currentLevel - 1) % themes.Length;
        LevelTheme activeTheme = themes[themeIndex];

        // --- ÇEVREYÝ BOYAMA ÝÞLEMÝNÝ ÇAÐIR ---
        UpdateEnvironmentVisuals(activeTheme);
        // -------------------------------------

        // Döngü hesaplanan uzunluðu kullanýyor
        for (int x = 0; x < currentCalculatedLength; x++)
        {
            int currentColHeight = Random.Range(currentMinHeight, currentMaxHeight + 1);
            int safeZoneY = currentColHeight - topLayerCount;

            for (int y = 0; y < currentColHeight; y++)
            {
                if (gridMap[x, y]) continue;

                // 1. BÜYÜK TAÞ
                bool wantBig = Random.value < bigStoneChance;
                bool canFitBig = (x < currentCalculatedLength - 1) && (y < currentColHeight - 1);
                bool isBelowTopLayer_Big = (y + 1) < safeZoneY;

                if (wantBig && canFitBig && isBelowTopLayer_Big && activeTheme.bigStones.Length > 0)
                {
                    if (!gridMap[x + 1, y] && !gridMap[x, y + 1] && !gridMap[x + 1, y + 1])
                    {
                        SpawnStoneFromList(activeTheme.bigStones, x, y, true);
                        gridMap[x, y] = true; gridMap[x + 1, y] = true;
                        gridMap[x, y + 1] = true; gridMap[x + 1, y + 1] = true;
                        totalStonesSpawned++;
                        continue;
                    }
                }

                // 2. ÖZEL TAÞ (RARE)
                bool wantRare = Random.value < rareStoneChance;
                bool isBelowTopLayer_Rare = y < safeZoneY;

                if (wantRare && isBelowTopLayer_Rare && activeTheme.rareStones.Length > 0)
                {
                    SpawnStoneFromList(activeTheme.rareStones, x, y, false);
                    gridMap[x, y] = true;
                    totalStonesSpawned++;
                    continue;
                }

                // 3. KÜÇÜK TAÞ
                bool isTopLayer = y >= safeZoneY;

                if (isTopLayer && activeTheme.topStonePrefabs.Length > 0)
                {
                    SpawnStoneFromList(activeTheme.topStonePrefabs, x, y, false);
                }
                else
                {
                    if (activeTheme.smallStones.Length > 0)
                        SpawnStoneFromList(activeTheme.smallStones, x, y, false);
                }

                gridMap[x, y] = true;
                totalStonesSpawned++;
            }
        }

        // Bitiþ çizgisi
        float endXPosition = CalculateWorldPos(currentCalculatedLength, 0).x + finishLineOffset;

        if (finishLinePrefab != null)
        {
            Vector3 finishPos = new Vector3(endXPosition, 0, 0);
            Instantiate(finishLinePrefab, finishPos, Quaternion.identity);
        }

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.SetLevelTarget(totalStonesSpawned);
        }
    }

    // --- YARDIMCI FONKSÝYONLAR (AYNI) ---
    void SpawnStoneFromList(GameObject[] sourceList, int xIndex, int yIndex, bool isBig)
    {
        if (sourceList == null || sourceList.Length == 0) return;
        GameObject prefab = sourceList[Random.Range(0, sourceList.Length)];

        Vector3 basePos = CalculateWorldPos(xIndex, yIndex);
        Vector3 finalPos = basePos;
        if (isBig) finalPos += new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0);

        GameObject newObj = CreateObject(prefab, finalPos);
        SetupStoneHealth(newObj, xIndex, isBig);
    }

    void SetupStoneHealth(GameObject obj, int xIndex, bool isBig)
    {
        StoneHealth hpScript = obj.GetComponent<StoneHealth>();
        if (hpScript != null)
        {
            int calculatedHp = CalculateHealth(isBig, xIndex);
            hpScript.SetHealth(calculatedHp);
        }
    }

    Vector3 CalculateWorldPos(int x, int y)
    {
        float camRightEdge = Camera.main.transform.position.x + (Camera.main.orthographicSize * Camera.main.aspect);
        float startX = camRightEdge + startXOffset;
        float posX = startX + (x * cellSize);
        float posY = transform.position.y + (y * cellSize);
        return new Vector3(posX, posY, 0);
    }

    GameObject CreateObject(GameObject prefab, Vector3 position)
    {
        GameObject obj = Instantiate(prefab, position, Quaternion.identity);
        StoneController sc = obj.GetComponent<StoneController>();
        if (sc == null) sc = obj.AddComponent<StoneController>();
        if (OptimizationManager.Instance != null) OptimizationManager.Instance.RegisterStone(obj);
        return obj;
    }

    [Header("Denge Ayarlarý")]
    public float baseStoneHP = 5f;
    public float hpGrowthFactor = 1.15f;
    public float bossHPMultiplier = 2.0f;

    int CalculateHealth(bool isBigStone, int xIndex)
    {
        int currentLevel = (LevelManager.Instance != null) ? LevelManager.Instance.currentLevel : 1;

        float levelBaseHP = baseStoneHP * Mathf.Pow(hpGrowthFactor, currentLevel - 1);

        if (currentLevel % 5 == 0)
        {
            levelBaseHP *= bossHPMultiplier;
        }

        // xIndex / currentCalculatedLength kullanarak oranla
        float progressPercent = (float)xIndex / (float)currentCalculatedLength;
        float distanceMultiplier = 1.0f + (progressPercent * 0.5f);

        float finalHPFloat = levelBaseHP * distanceMultiplier;

        if (isBigStone)
        {
            finalHPFloat *= bigStoneHealthMult;
        }

        return Mathf.Max(1, Mathf.RoundToInt(finalHPFloat));
    }

    public void CalculateCurrentLevelMaxHeight()
    {
        int currentLevel = (LevelManager.Instance != null) ? LevelManager.Instance.currentLevel : 1;
        int calculatedMax = startMaxHeight + ((currentLevel - 1) / increaseEveryXLevel);
        CurrentLevelMaxHeight = Mathf.Clamp(calculatedMax, startMaxHeight, absoluteMaxHeight);
    }
}