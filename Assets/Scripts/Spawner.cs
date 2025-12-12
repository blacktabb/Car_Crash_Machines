using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject stonePrefab;
    public float spawnInterval = 3f;
    public float spawnX = 12f; // Sað tarafta spawn noktasý

    // Yükseklik ayarlarý
    public float groundY = -4f; // Zeminin Y seviyesi (bunu sahnene göre ayarla)
    public float stoneHeight = 1f; // Taþýn yüksekliði (Sprite boyutuna göre)

    void Start()
    {
        InvokeRepeating(nameof(SpawnTower), 1f, spawnInterval);
    }

    void SpawnTower()
    {
        // Rastgele kaç katlý olacaðýný seç (Örn: 1 ile 4 kat arasý)
        int floorCount = Random.Range(1, 5);

        for (int i = 0; i < floorCount; i++)
        {
            // Üst üste pozisyon hesapla
            // i * stoneHeight: Her döngüde bir taþ boyu yukarý çýk
            Vector2 spawnPos = new Vector2(spawnX, groundY + (i * stoneHeight) + 0.5f);

            Instantiate(stonePrefab, spawnPos, Quaternion.identity);
        }
    }
}
