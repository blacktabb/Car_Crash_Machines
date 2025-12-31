using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Oyun Hýzý")]
    public float gameSpeed = 5f; // Taþlarýn sana gelme hýzý

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}