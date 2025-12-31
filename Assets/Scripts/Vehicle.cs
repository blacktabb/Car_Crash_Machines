using UnityEngine;

public class Vehicle : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        // Eðer çarpan þey bir "Taþ" ise
        if (collision.gameObject.CompareTag("Stone"))
        {
            Debug.Log("Oyun Bitti! Taþ arabaya çarptý.");
            // Time.timeScale = 0; // Oyunu durdurmak istersen
            // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Yeniden baþlatmak için
        }
    }
}