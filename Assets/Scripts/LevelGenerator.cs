using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [System.Serializable]
    public struct LevelTheme
    {
        public string themeName;

        [Header("Özel Bölge Taþlarý")]
        public GameObject[] topStonePrefabs; // En üstte çýkacaklar (Çim vb.)

        [Header("Standart Taþlar")]
        public GameObject[] smallStones;     // Alt kýsýmlar
        public GameObject[] bigStones;
        public GameObject[] rareStones;
    }

    [Header("Tema Sistemi")]
    public LevelTheme[] themes;

    [Header("Görsel Ayarlar")]
    public int topLayerCount = 1;

    [Header("Dinamik Yükseklik Ayarlarý")]
    public int startMaxHeight = 3;     // Level 1'de kaç kat?
    public int absoluteMaxHeight = 12; // TAVAN SINIRI (Eski maxHeight yerine bunu kullanacaðýz)
    public int increaseEveryXLevel = 3; // Kaç levelde bir artsýn?
    public int heightVariation = 2;     // Dalgalanma miktarý

    [Header("Level Ayarlarý")]
    public int levelLength = 50;
    public static LevelGenerator Instance;
    public int CurrentLevelMaxHeight { get; private set; }

    // NOT: Eski minHeight ve maxHeight deðiþkenlerini kaldýrdýk.
    // Artýk dinamik hesaplanýyorlar.

    [Header("Grid Ayarlarý")]
    public float cellSize = 1f;
    public float startXOffset = 10f;
    public float scrollSpeed = 2f;

    [Header("Þans")]
    [Range(0f, 1f)] public float bigStoneChance = 0.3f;
    [Range(0f, 1f)] public float rareStoneChance = 0.05f;

    [Header("Zorluk Ayarlarý")]
    public int baseHealth = 3;
    public float levelMultiplier = 2.5f;
    public int bigStoneHealthMult = 3;

    private bool[,] gridMap;

    void Start()
    {
        // --- DÜZELTME BURADA YAPILDI ---
        // Eskiden: new bool[levelLength, maxHeight + 5]; yazýyordu.
        // Artýk "absoluteMaxHeight" kullanýyoruz ki hafýzada en geniþ yeri ayýrsýn.
        gridMap = new bool[levelLength, absoluteMaxHeight + 5];

        GenerateGridAndSpawn();
    }

    void Awake()
    {
        if (Instance == null) Instance = this;

        CalculateCurrentLevelMaxHeight();
    }

    void GenerateGridAndSpawn()
    {
        int totalStonesSpawned = 0;
        int currentLevel = (LevelManager.Instance != null) ? LevelManager.Instance.currentLevel : 1;
        int currentMaxHeight = CurrentLevelMaxHeight;

        // --- DÝNAMÝK YÜKSEKLÝK HESABI ---       
        int currentMinHeight = Mathf.Max(1, currentMaxHeight - heightVariation);
        // --------------------------------

        int themeIndex = (currentLevel - 1) % themes.Length;
        LevelTheme activeTheme = themes[themeIndex];

        for (int x = 0; x < levelLength; x++)
        {
            // Sütun yüksekliðini belirle
            int currentColHeight = Random.Range(currentMinHeight, currentMaxHeight + 1);

            // SafeZone (Üst katman korumasý)
            int safeZoneY = currentColHeight - topLayerCount;

            for (int y = 0; y < currentColHeight; y++)
            {
                if (gridMap[x, y]) continue;

                // 1. BÜYÜK TAÞ KONTROLÜ
                bool wantBig = Random.value < bigStoneChance;
                bool canFitBig = (x < levelLength - 1) && (y < currentColHeight - 1);
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

                // 2. ÖZEL TAÞ (RARE) KONTROLÜ
                bool wantRare = Random.value < rareStoneChance;
                bool isBelowTopLayer_Rare = y < safeZoneY;

                if (wantRare && isBelowTopLayer_Rare && activeTheme.rareStones.Length > 0)
                {
                    SpawnStoneFromList(activeTheme.rareStones, x, y, false);
                    gridMap[x, y] = true;
                    totalStonesSpawned++;
                    continue;
                }

                // 3. KÜÇÜK TAÞ / ÜST KATMAN TAÞI
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

        float progressPercent = (float)xIndex / (float)levelLength;
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

        // Formül
        int calculatedMax = startMaxHeight + ((currentLevel - 1) / increaseEveryXLevel);

        // Sýnýrla ve Kaydet
        CurrentLevelMaxHeight = Mathf.Clamp(calculatedMax, startMaxHeight, absoluteMaxHeight);

        Debug.Log($"LEVEL: {currentLevel} | HESAPLANAN TAVAN YÜKSEKLÝÐÝ: {CurrentLevelMaxHeight}");
    }
}