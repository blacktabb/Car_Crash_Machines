using UnityEngine;
using System.Collections.Generic;

public class EndlessTerrainManager : MonoBehaviour
{
    public static EndlessTerrainManager Instance;

    // --- YENÝ: TEMA YAPISI ---
    [System.Serializable]
    public struct TerrainTheme
    {
        public string themeName;
        [Header("Zemin Modeli")]
        public GameObject groundModelPrefab;

        [Header("Kenar Modelleri (Listeler)")]
        public GameObject[] leftBorderPrefabs;
        public GameObject[] rightBorderPrefabs;
        public GameObject[] leftFarBorderPrefabs;
        public GameObject[] leftFarFarBorderPrefabs;
    }

    [Header("Tema Listesi")]
    public TerrainTheme[] themes; // Level 1 -> 0. Eleman, Level 2 -> 1. Eleman...

    // Not: Boyut ve mesafe ayarlarýný global tutuyoruz (Tüm temalar ayný boyutta olmalý)
    // Eðer modellerin boyutu çok deðiþiyorsa bunlarý da Theme struct'ýnýn içine taþýyabiliriz.
    [Header("Boyut Ayarlarý (Tüm Temalar Ýçin)")]
    public float roadSegmentLength = 10f;
    public float leftBorderLength = 10f;
    public float rightBorderLength = 10f;
    public float leftFarBorderLength = 20f;
    public float leftFarFarBorderLength = 20f;

    [Header("Mesafe (Offset) Ayarlarý")]
    public float sideOffset = 10f;
    public float farSideOffset = 25f;
    public float farFarSideOffset = 35f; // Arkadakiler çakýþmasýn diye biraz artýrdým

    [Header("Yok Olma Sýnýrý")]
    public float recycleXPosition = -30f;

    // Listeler
    private List<Transform> roadSegments = new List<Transform>();
    private List<Transform> leftSegments = new List<Transform>();
    private List<Transform> rightSegments = new List<Transform>();
    private List<Transform> leftFarSegments = new List<Transform>();
    private List<Transform> leftFarFarSegments = new List<Transform>();

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        SpawnLevelRoad();
    }

    public void SpawnLevelRoad()
    {
        // Temizlik
        ClearList(roadSegments);
        ClearList(leftSegments);
        ClearList(rightSegments);
        ClearList(leftFarSegments);
        ClearList(leftFarFarSegments);

        // Level Bilgisini Al
        int levelLen = 100;
        int currentLevel = 1;

        if (LevelManager.Instance != null)
        {
            currentLevel = LevelManager.Instance.currentLevel;
        }

        if (LevelGenerator.Instance != null)
        {
            levelLen = LevelGenerator.Instance.currentCalculatedLength;
        }
        float totalTargetLength = levelLen + 200f; // Fazlalýk payý

        float currentY = transform.position.y;
        float currentZ = transform.position.z;

        // --- TEMA SEÇÝMÝ ---
        TerrainTheme activeTheme = new TerrainTheme();

        if (themes != null && themes.Length > 0)
        {
            // Level 1 -> Index 0
            // Eðer tema sayýsý level sayýsýndan azsa baþa sar (Modülüs iþlemi)
            int themeIndex = (currentLevel - 1) % themes.Length;
            activeTheme = themes[themeIndex];
        }
        else
        {
            Debug.LogError("EndlessTerrainManager: Tema listesi boþ! Lütfen Inspector'dan tema ekleyin.");
            return;
        }

        // 1. ANA YOLU OLUÞTUR (Temadan gelen prefab ile)
        SpawnLane(activeTheme.groundModelPrefab, roadSegmentLength, totalTargetLength, 0, roadSegments, false);

        // 2. SOL KENAR
        SpawnLaneRandom(activeTheme.leftBorderPrefabs, leftBorderLength, totalTargetLength, sideOffset, leftSegments);

        // 3. SAÐ KENAR (-Offset)
        SpawnLaneRandom(activeTheme.rightBorderPrefabs, rightBorderLength, totalTargetLength, -sideOffset, rightSegments);

        // 4. SOL UZAK KENAR
        SpawnLaneRandom(activeTheme.leftFarBorderPrefabs, leftFarBorderLength, totalTargetLength, farSideOffset, leftFarSegments);

        // 5. SOL 2X UZAK KENAR
        SpawnLaneRandom(activeTheme.leftFarFarBorderPrefabs, leftFarFarBorderLength, totalTargetLength, farFarSideOffset, leftFarFarSegments);
    }

    // --- TEK TÝP PREFAB OLUÞTURUCU (ANA YOL ÝÇÝN) ---
    void SpawnLane(GameObject prefab, float segmentLen, float totalLen, float zOffset, List<Transform> listToFill, bool randomYRotation = false)
    {
        if (prefab == null) return;

        int count = Mathf.CeilToInt(totalLen / segmentLen);
        float currentY = transform.position.y;
        float currentZ = transform.position.z + zOffset;

        for (int i = 0; i < count; i++)
        {
            float xPos = i * segmentLen;
            GameObject obj = Instantiate(prefab, transform);
            obj.transform.position = new Vector3(xPos, currentY, currentZ);

            if (randomYRotation)
                obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 4) * 90, 0);

            AddMover(obj);
            listToFill.Add(obj.transform);
        }
    }

    // --- RASTGELE PREFAB OLUÞTURUCU (KENARLAR ÝÇÝN) ---
    void SpawnLaneRandom(GameObject[] prefabs, float segmentLen, float totalLen, float zOffset, List<Transform> listToFill)
    {
        if (prefabs == null || prefabs.Length == 0) return;

        int count = Mathf.CeilToInt(totalLen / segmentLen);
        float currentY = transform.position.y;
        float currentZ = transform.position.z + zOffset;

        for (int i = 0; i < count; i++)
        {
            GameObject selectedPrefab = prefabs[Random.Range(0, prefabs.Length)];
            float xPos = i * segmentLen;

            GameObject obj = Instantiate(selectedPrefab, transform);
            obj.transform.position = new Vector3(xPos, currentY, currentZ);

            AddMover(obj);
            listToFill.Add(obj.transform);
        }
    }

    void AddMover(GameObject obj)
    {
        if (obj.GetComponent<MoveWithWorld>() == null)
            obj.AddComponent<MoveWithWorld>();
    }

    void Update()
    {
        CheckLane(roadSegments);
        CheckLane(leftSegments);
        CheckLane(rightSegments);
        CheckLane(leftFarSegments);
        CheckLane(leftFarFarSegments);
    }

    void CheckLane(List<Transform> laneList)
    {
        if (laneList.Count == 0) return;

        if (laneList[0] == null)
        {
            laneList.RemoveAt(0);
            return;
        }

        if (laneList[0].position.x < recycleXPosition)
        {
            Destroy(laneList[0].gameObject);
            laneList.RemoveAt(0);
        }
    }

    void ClearList(List<Transform> list)
    {
        foreach (var item in list)
        {
            if (item != null) Destroy(item.gameObject);
        }
        list.Clear();
    }
}