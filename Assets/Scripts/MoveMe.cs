using UnityEngine;

public class MoveWithWorld : MonoBehaviour
{
    void Start()
    {
        // Doðduðumda beni hareket listesine ekle
        if (WorldMover.Instance != null)
        {
            WorldMover.Instance.RegisterObject(this.transform);
        }
    }

    void OnDestroy()
    {
        // Yok olursam beni listeden sil
        if (WorldMover.Instance != null)
        {
            WorldMover.Instance.UnregisterObject(this.transform);
        }
    }
}