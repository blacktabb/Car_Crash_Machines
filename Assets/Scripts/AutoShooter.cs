using UnityEngine;

public class AutoShooter : MonoBehaviour
{
    public GameObject bulletPrefab; // Mermi prefabýný buraya sürükle
    public Transform firePoint;     // Merminin çýkacaðý namlu ucu (Boþ bir GameObject)
    public float fireRate = 0.2f;   // Saniyede kaç mermi (Daha düþük = Daha hýzlý)

    private float nextFireTime = 0f;

    void Update()
    {
        // Zamaný geldi mi?
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        if (firePoint != null && bulletPrefab != null)
        {
            Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        }
    }
}