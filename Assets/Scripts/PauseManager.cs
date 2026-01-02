using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Pause")]
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject confirmResetProgressPanel;
    bool isPaused;

    [Header("Sound")]
    [SerializeField] Image soundButtonImage;
    [SerializeField] Sprite soundOnSprite;
    [SerializeField] Sprite soundOffSprite;

    bool isSoundOn = true;

    void Start()
    {
        // Pause
        pausePanel.SetActive(false);
        confirmResetProgressPanel.SetActive(false);
        isPaused = false;
        Time.timeScale = 1f;

        // Sound (kaydedilmiþ ayarý yükle)
        isSoundOn = PlayerPrefs.GetInt("Sound", 1) == 1;
        ApplySoundState();
    }

    // ---------------- PAUSE ----------------

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    // ---------------- SOUND ----------------

    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        ApplySoundState();

        PlayerPrefs.SetInt("Sound", isSoundOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    void ApplySoundState()
    {
        AudioListener.volume = isSoundOn ? 1f : 0f;
        soundButtonImage.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
    }

    // ---------------- RESET ----------------

    public void ConfirmResetProgress()
    {
        confirmResetProgressPanel.SetActive(true);
    }
    public void CancelResetProgress()
    {
        confirmResetProgressPanel.SetActive(false);
    }
    public void ResetProgress()
    {
        Debug.Log("TÜM ÝLERLEME SÝLÝNÝYOR... SIFIRDAN BAÞLATILIYOR.");
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
