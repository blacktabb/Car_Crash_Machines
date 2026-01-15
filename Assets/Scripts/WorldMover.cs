using UnityEngine;
using System.Collections.Generic;

public class WorldMover : MonoBehaviour
{
    public static WorldMover Instance;

    [Header("Hareket Ayarlarý")]
    public Vector3 moveDirection = Vector3.left;

    // Hareket etmesi gereken tüm objelerin listesi (Zeminler, Taþlar, FinishLine)
    private List<Transform> movableObjects = new List<Transform>();

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // Objeler doðduðunda kendini buraya kaydettirecek
    public void RegisterObject(Transform obj)
    {
        if (!movableObjects.Contains(obj))
        {
            movableObjects.Add(obj);
        }
    }

    // Objeler yok olduðunda listeden çýkacak
    public void UnregisterObject(Transform obj)
    {
        if (movableObjects.Contains(obj))
        {
            movableObjects.Remove(obj);
        }
    }

    void Update()
    {
        // GameManager yoksa veya oyun hýzý 0 ise hareket etme
        if (GameManager.Instance == null || GameManager.Instance.gameSpeed <= 0) return;

        float speed = GameManager.Instance.gameSpeed;
        Vector3 movement = moveDirection * speed * Time.deltaTime;

        // Listeyi tersten dönüyoruz (Silinme ihtimaline karþý güvenli)
        for (int i = movableObjects.Count - 1; i >= 0; i--)
        {
            // Eðer obje yok edildiyse listeden temizle
            if (movableObjects[i] == null)
            {
                movableObjects.RemoveAt(i);
                continue;
            }

            // Objeyi kaydýr
            movableObjects[i].Translate(movement, Space.World);
        }
    }
}