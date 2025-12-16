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

    [Header("Zorluk Ayarlarý (Aralýklý)")]
    // Küçük taþlarýn baþlangýç aralýðý (Örn: 2 ile 6)
    public Vector2Int smallRange = new Vector2Int(2, 6);

    // Büyük taþlarýn baþlangýç aralýðý (Örn: 8 ile 12)
    public Vector2Int bigRange = new Vector2Int(8, 12);

    public float increaseInterval = 5f; // Kaç saniyede bir artsýn? (5 sn)
    public int increaseAmount = 1;      // Aralýklar ne kadar kayacak?

    [Header("UI & Debug")]
    public TextMeshProUGUI difficultyText; // Canvas'taki yazý

    // Private Deðiþkenler
    private float spawnX;
    private bool[] nextColumnReserved;
    private float virtualCursorX;

    private float difficultyTimer = 0f;
    private int difficultyOffset = 0; // Baþlangýçtan beri ne kadar arttýk?

    void Start()
    {
        // --- KAMERA HESAPLARI ---
        float cameraHeight = 2f * Camera.main.orthographicSize;
        float cameraWidth = cameraHeight * Camera.main.aspect;
        spawnX = Camera.main.transform.position.x + (cameraWidth / 2f) + 2f;

        nextColumnReserved = new bool[rows];
        virtualCursorX = spawnX;

        UpdateDifficultyUI();
    }

    void Update()
    {
        // 1. ZORLUK ZAMANLAYICISI
        difficultyTimer += Time.deltaTime;

        // 5 Saniye doldu mu?
        if (difficultyTimer >= increaseInterval)
        {
            difficultyTimer = 0f;

            // Ofseti artýr (Bu deðer min ve max'a eklenecek)
            difficultyOffset += increaseAmount;

            UpdateDifficultyUI();
        }

        // 2. SPAWN SÝSTEMÝ
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
        for (int i = 0; i < rows; i++) nextColumnReserved[i] = false;
        float currentYBase = transform.position.y;

        for (int y = 0; y < rows; y++)
        {
            if (currentColumnReserved[y]) continue;

            bool canSpawnBig = (y < rows - 1) && !currentColumnReserved[y + 1];
            bool chooseBig = canSpawnBig && (Random.value < 0.2f);

            if (chooseBig)
            {
                // BÜYÜK TAÞ CAN HESABI
                // Baþlangýç deðerlerine ofseti ekliyoruz
                int min = bigRange.x + difficultyOffset;
                int max = bigRange.y + difficultyOffset;

                // Random.Range(min, max + 1) -> max dahil olsun diye +1 ekleriz
                int randomHealth = Random.Range(min, max + 1);

                SpawnBigStone(xPos, y, currentYBase, randomHealth);

                currentColumnReserved[y + 1] = true;
                nextColumnReserved[y] = true;
                nextColumnReserved[y + 1] = true;
            }
            else
            {
                // KÜÇÜK TAÞ CAN HESABI
                int min = smallRange.x + difficultyOffset;
                int max = smallRange.y + difficultyOffset;

                int randomHealth = Random.Range(min, max + 1);

                SpawnSmallStone(xPos, y, currentYBase, randomHealth);
            }
        }
    }

    void SpawnSmallStone(float x, int yIndex, float baseY, int health)
    {
        float yPos = baseY + (yIndex * cellSize);
        Vector2 pos = new Vector2(x, yPos + (cellSize / 2f));
        GameObject stone = Instantiate(smallStone, pos, Quaternion.identity);
        stone.GetComponent<StoneHealth>().SetHealth(health);
    }

    void SpawnBigStone(float x, int yIndex, float baseY, int health)
    {
        float yPos = baseY + (yIndex * cellSize);
        Vector2 pos = new Vector2(x + (cellSize * 0.5f), yPos + cellSize);
        GameObject stone = Instantiate(bigStone, pos, Quaternion.identity);
        stone.GetComponent<StoneHealth>().SetHealth(health);
    }

    void UpdateDifficultyUI()
    {
        if (difficultyText != null)
        {
            // Oyuncuya sadece küçük taþlarýn aralýðýný göstermek yeterlidir
            // Örn: "Zone: 3 - 7 HP"
            int currentMin = smallRange.x + difficultyOffset;
            int currentMax = smallRange.y + difficultyOffset;

            difficultyText.text = $"Zone HP: {currentMin} - {currentMax}";
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        float lineY = transform.position.y + (cellSize / 2f);
        Vector3 startLine = new Vector3(transform.position.x - 10, lineY, 0);
        Vector3 endLine = new Vector3(transform.position.x + 20, lineY, 0);
        Gizmos.DrawLine(startLine, endLine);
    }
}