using UnityEngine;
using TMPro;

public class Spawner : MonoBehaviour
{
    [Header("Prefablar")]
    public GameObject smallStone;
    public GameObject bigStone;

    [Header("Grid Ayarlarý")]
    public int rows = 6;
    public float cellSize = 1f;

    [Header("Zorluk Ayarlarý")]
    public Vector2Int smallRange = new Vector2Int(2, 6);
    public Vector2Int bigRange = new Vector2Int(8, 12);
    public float increaseInterval = 5f;
    public int increaseAmount = 1;

    [Header("UI")]
    public TextMeshProUGUI difficultyText;

    // --- YENÝ EKLENENLER (LÝMÝT SÝSTEMÝ) ---
    private int totalSpawnedStones = 0; // Bu levelde kaç taþ doðdu?
    private bool spawningFinished = false; // Limit doldu mu?
    // ---------------------------------------

    // Private Deðiþkenler
    private float spawnX;
    private bool[] nextColumnReserved;
    private float virtualCursorX;

    private float difficultyTimer = 0f;
    private int difficultyOffset = 0;

    void Start()
    {
        CalculateCameraBounds();
        nextColumnReserved = new bool[rows];

        // Oyuna baþladýðýmýzda (veya level yüklendiðinde) sayacý sýfýrla
        ResetSpawner();
    }

    void CalculateCameraBounds()
    {
        float cameraHeight = 2f * Camera.main.orthographicSize;
        float cameraWidth = cameraHeight * Camera.main.aspect;
        spawnX = Camera.main.transform.position.x + (cameraWidth / 2f) + 2f;
        virtualCursorX = spawnX;
    }

    // Level deðiþtiðinde LevelManager bunu çaðýrýp sýfýrlayabilir
    public void ResetSpawner()
    {
        totalSpawnedStones = 0;
        spawningFinished = false;
        difficultyOffset = 0;
        difficultyTimer = 0f;
        UpdateDifficultyUI();

        // Sanal imleci tekrar kamera saðýna taþý
        CalculateCameraBounds();
    }

    void Update()
    {
        // --- 1. KONTROL: YETERÝNCE TAÞ DOÐDU MU? ---
        if (LevelManager.Instance != null)
        {
            // Eðer doðan taþ sayýsý, hedeften büyük veya eþitse DUR.
            // +2 ekliyoruz ki ekran bomboþ kalmasýn, garanti olsun.
            if (totalSpawnedStones >= LevelManager.Instance.GetTargetProgress())
            {
                spawningFinished = true;
            }
        }

        // Eðer üretim bittiyse fonksiyonu burada kes, aþaðý inme.
        if (spawningFinished) return;
        // --------------------------------------------------

        // 2. ZORLUK ZAMANLAYICISI
        difficultyTimer += Time.deltaTime;
        if (difficultyTimer >= increaseInterval)
        {
            difficultyTimer = 0f;
            difficultyOffset += increaseAmount;
            UpdateDifficultyUI();
        }

        // 3. SPAWN SÝSTEMÝ
        virtualCursorX -= GameManager.Instance.gameSpeed * Time.deltaTime;

        if (spawnX - virtualCursorX >= cellSize)
        {
            SpawnColumn(spawnX);
            virtualCursorX += cellSize;
        }
    }

    void SpawnColumn(float xPos)
    {
        bool[] currentColumnReserved = (bool[])nextColumnReserved.Clone();

        // Gelecek sütun rezervasyonlarýný temizle
        for (int i = 0; i < rows; i++) nextColumnReserved[i] = false;

        float currentYBase = transform.position.y;

        // --- DEÐÝÞÝKLÝK BURADA ---
        // Standart "rows" (6) yerine, o anlýk rastgele bir yükseklik belirliyoruz.
        // Random.Range(min, max) tam sayýlarda max deðerini dahil etmez.
        // Yani (3, rows + 1) yazarsak -> 3, 4, 5, 6 deðerlerini alabilir.
        int currentColumnHeight = Random.Range(3, rows + 1);
        // -------------------------

        // Döngü sýnýrýný 'rows' yerine 'currentColumnHeight' yaptýk
        for (int y = 0; y < currentColumnHeight; y++)
        {
            if (currentColumnReserved[y]) continue;

            // Büyük taþ kontrolü (En tepeye gelmediysek ve yer varsa)
            // Not: y < rows - 1 kontrolünü koruyoruz ki grid dýþýna taþmasýn.
            bool canSpawnBig = (y < rows - 1) && !currentColumnReserved[y + 1];
            bool chooseBig = canSpawnBig && (Random.value < 0.2f);

            if (chooseBig)
            {
                int min = bigRange.x + difficultyOffset;
                int max = bigRange.y + difficultyOffset;
                SpawnBigStone(xPos, y, currentYBase, Random.Range(min, max + 1));

                currentColumnReserved[y + 1] = true;
                nextColumnReserved[y] = true;
                nextColumnReserved[y + 1] = true;
            }
            else
            {
                int min = smallRange.x + difficultyOffset;
                int max = smallRange.y + difficultyOffset;
                SpawnSmallStone(xPos, y, currentYBase, Random.Range(min, max + 1));
            }
        }
    }

    void SpawnSmallStone(float x, int yIndex, float baseY, int health)
    {
        float yPos = baseY + (yIndex * cellSize);
        Vector2 pos = new Vector2(x, yPos + (cellSize / 2f));
        GameObject stone = Instantiate(smallStone, pos, Quaternion.identity);
        stone.GetComponent<StoneHealth>().SetHealth(health);

        // SAYACI ARTIR
        totalSpawnedStones++;
    }

    void SpawnBigStone(float x, int yIndex, float baseY, int health)
    {
        float yPos = baseY + (yIndex * cellSize);
        Vector2 pos = new Vector2(x + (cellSize * 0.5f), yPos + cellSize);
        GameObject stone = Instantiate(bigStone, pos, Quaternion.identity);
        stone.GetComponent<StoneHealth>().SetHealth(health);

        // SAYACI ARTIR (Büyük taþ da 1 puan sayýlýyor)
        totalSpawnedStones++;
    }

    void UpdateDifficultyUI()
    {
        if (difficultyText != null)
        {
            int currentMin = smallRange.x + difficultyOffset;
            int currentMax = smallRange.y + difficultyOffset;
            difficultyText.text = $"Zone HP: {currentMin} - {currentMax}";
        }
    }
}