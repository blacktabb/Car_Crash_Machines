using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton yapýsý (Her yerden ulaþmak için)
    public static GameManager Instance;

    public float gameSpeed = 5f; // Tüm oyunun akýþ hýzý

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}