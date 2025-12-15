using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Prefablar")]
    public GameObject smallStone;
    public GameObject bigStone;

    [Header("Grid Ayarlarý")]
    public int rows = 6;
    public float cellSize = 1f;

    // 'startY' deðiþkenini kaldýrdýk. Artýk transform.position.y kullanacaðýz.

    private float spawnX;
    private bool[] nextColumnReserved;
    private float virtualCursorX;

    void Start()
    {
        // 1. KAMERA SAÐ KENAR HESABI
        float cameraHeight = 2f * Camera.main.orthographicSize;
        float cameraWidth = cameraHeight * Camera.main.aspect;
        spawnX = Camera.main.transform.position.x + (cameraWidth / 2f) + 2f;

        nextColumnReserved = new bool[rows];
        virtualCursorX = spawnX;
    }

    void Update()
    {
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

        // --- DEÐÝÞÝKLÝK BURADA: Baþlangýç noktasý objenin kendisi ---
        float currentYBase = transform.position.y;

        for (int y = 0; y < rows; y++)
        {
            if (currentColumnReserved[y]) continue;

            bool canSpawnBig = (y < rows - 1) && !currentColumnReserved[y + 1];
            bool chooseBig = canSpawnBig && (Random.value < 0.2f);

            if (chooseBig)
            {
                SpawnBigStone(xPos, y, currentYBase); // Base Y'yi gönderiyoruz
                currentColumnReserved[y + 1] = true;
                nextColumnReserved[y] = true;
                nextColumnReserved[y + 1] = true;
            }
            else
            {
                SpawnSmallStone(xPos, y, currentYBase);
            }
        }
    }

    // Fonksiyonlara 'baseY' parametresi ekledik
    void SpawnSmallStone(float x, int yIndex, float baseY)
    {
        // startY yerine baseY kullanýyoruz
        float yPos = baseY + (yIndex * cellSize);
        Vector2 pos = new Vector2(x, yPos + (cellSize / 2f));
        Instantiate(smallStone, pos, Quaternion.identity);
    }

    void SpawnBigStone(float x, int yIndex, float baseY)
    {
        float yPos = baseY + (yIndex * cellSize);
        Vector2 pos = new Vector2(x + (cellSize * 0.5f), yPos + cellSize);
        Instantiate(bigStone, pos, Quaternion.identity);
    }

    // GÖRSEL YARDIMCI (GÝZMO)
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        // Çizgi artýk transform.position.y hizasýndan baþlar
        float lineY = transform.position.y + (cellSize / 2f);

        Vector3 startLine = new Vector3(transform.position.x - 10, lineY, 0);
        Vector3 endLine = new Vector3(transform.position.x + 20, lineY, 0);

        Gizmos.DrawLine(startLine, endLine);

        // Kutu
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawCube(new Vector3(transform.position.x, lineY, 0), new Vector3(cellSize, cellSize, 1));
    }
}