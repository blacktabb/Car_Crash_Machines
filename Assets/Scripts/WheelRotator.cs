using UnityEngine;

public class WheelRotator : MonoBehaviour
{
    [Header("Ayarlar")]
    public float rotateSpeed = 300f; // Dönüþ hýzý
    public bool reverseRotation = false; // Ters dönüyorsa bunu iþaretle

    // Dönüþ Eksenleri (Modeline göre deðiþebilir, genelde X eksenidir)
    public Vector3 rotationAxis = Vector3.right;

    void Update()
    {
        // Oyun durduysa (GameOver/Win) dönmeyi býrak
        if (Time.timeScale == 0) return;

        float speed = reverseRotation ? -rotateSpeed : rotateSpeed;

        // Saniyede 'speed' derece kadar döndür
        transform.Rotate(rotationAxis * speed * Time.deltaTime);
    }
}