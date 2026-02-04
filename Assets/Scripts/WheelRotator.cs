using UnityEngine;

public class WheelRotator : MonoBehaviour
{
    [Header("Ayarlar")]
    public float rotateSpeed = 300f; // Temel dönüþ hýzý
    public bool reverseRotation = false; // Ters dönüyorsa iþaretle

    // Dönüþ Eksenleri (Modeline göre deðiþebilir, genelde X eksenidir)
    public Vector3 rotationAxis = Vector3.right;

    void Update()
    {
        // 1. GameManager Kontrolü
        // Eðer GameManager yoksa veya Oyun Hýzý 0 ise (Tap to Play ekranýndaysak)
        // Hiçbir þey yapma, dönme.
        if (GameManager.Instance == null || GameManager.Instance.gameSpeed <= 0f)
            return;

        // 2. Yön Belirleme
        float baseSpeed = reverseRotation ? -rotateSpeed : rotateSpeed;

        // 3. Hýz Senkronizasyonu (Bu kýsým çok önemli!)
        // Tekerleðin dönüþ hýzýný, oyunun akýþ hýzýyla çarparak senkronize ediyoruz.
        // Böylece "Slow Motion" reklamý izlendiðinde tekerlekler de aðýr çekimde döner.
        // GameManager'daki targetSpeed'e bölerek bir oran (0 ile 1 arasý) buluyoruz.

        float speedRatio = 1f;
        if (GameManager.Instance.targetSpeed > 0)
        {
            speedRatio = GameManager.Instance.gameSpeed / GameManager.Instance.targetSpeed;
        }

        // 4. Döndürme Ýþlemi
        transform.Rotate(rotationAxis * baseSpeed * speedRatio * Time.deltaTime);
    }
}