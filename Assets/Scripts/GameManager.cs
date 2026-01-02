using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    Coroutine slowRoutine;
    float defaultSpeed;


    [Header("Oyun Hýzý")]
    public float gameSpeed = 1f; // Taþlarýn sana gelme hýzý

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 20 sn yarý hýz
    public void SlowGame(float duration)
    {
        if (slowRoutine != null)
            StopCoroutine(slowRoutine);

        slowRoutine = StartCoroutine(SlowRoutine(duration));
    }

    IEnumerator SlowRoutine(float duration)
    {
        gameSpeed = gameSpeed * 0.5f;
        yield return new WaitForSeconds(duration);
        gameSpeed = gameSpeed * 2f;
        slowRoutine = null;
    }
}