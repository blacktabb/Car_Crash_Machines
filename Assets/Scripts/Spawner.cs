using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Spawn Ayarlarý")]
    public GameObject stonePrefab;
    public float spawnInterval = 3f;
    public float spawnX = 12f;
    public float groundY = -4f;
    public float stoneHeight = 1f;

    [Header("Zorluk (Difficulty)")]
    public int startHealth = 5;          // Oyun baþýndaki taþ caný
    public int increaseAmount = 2;       // Her spawn'da eklenecek can miktarý

    private int spawnCount = 0;          // Kaçýncý dalgayý spawn ediyoruz?

    void Start()
    {
        InvokeRepeating(nameof(SpawnTower), 1f, spawnInterval);
    }

    void SpawnTower()
    {
        // 1. ZORLUK HESAPLAMA
        // Formül: Baþlangýç + (Spawn Sayýsý * Artýþ Miktarý)
        // Örn: 5 + (0*2)=5 -> 5 + (1*2)=7 -> 5 + (2*2)=9 ...
        int currentDifficultyHealth = startHealth + (spawnCount * increaseAmount);

        // Kule yüksekliðini rastgele seç
        int floorCount = Random.Range(1, 5);

        for (int i = 0; i < floorCount; i++)
        {
            Vector2 spawnPos = new Vector2(spawnX, groundY + (i * stoneHeight) + 0.5f);

            GameObject newStone = Instantiate(stonePrefab, spawnPos, Quaternion.identity);

            // 2. TAÞA YENÝ CAN DEÐERÝNÝ GÖNDER
            StoneHealth healthScript = newStone.GetComponent<StoneHealth>();
            if (healthScript != null)
            {
                healthScript.SetHealth(currentDifficultyHealth);
            }
        }

        // Bir sonraki dalga için sayacý artýr
        spawnCount++;
    }
}