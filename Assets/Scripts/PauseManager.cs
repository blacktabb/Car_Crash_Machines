using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Pause")]
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject confirmResetProgressPanel;
    bool isPaused;

    [Header("Sound (SFX Only)")]
    [SerializeField] Image soundButtonImage;
    [SerializeField] Sprite soundOnSprite;
    [SerializeField] Sprite soundOffSprite;

    bool isSoundOn = true;

    void Start()
    {
        // Pause Baþlangýç Ayarlarý
        if (pausePanel != null) pausePanel.SetActive(false);
        if (confirmResetProgressPanel != null) confirmResetProgressPanel.SetActive(false);

        isPaused = false;
        Time.timeScale = 1f;

        // Sound (kaydedilmiþ ayarý yükle)
        isSoundOn = PlayerPrefs.GetInt("Sound", 1) == 1;
        ApplySoundState();
    }

    // ---------------- PAUSE ----------------

    public void PauseGame()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
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

    // ---------------- SOUND (SADECE EFEKT) ----------------

    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        ApplySoundState();

        PlayerPrefs.SetInt("Sound", isSoundOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    void ApplySoundState()
    {
        // --- DÜZELTME BURADA ---
        // Eskiden: AudioListener.volume = ... (Hepsini kapatýyordu)
        // Þimdi: Sadece AudioManager'daki SFX kanalýný kapatýyoruz.

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXState(isSoundOn);
        }

        if (soundButtonImage != null)
        {
            soundButtonImage.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
        }
    }

    // ---------------- RESET ----------------

    public void ConfirmResetProgress()
    {
        if (confirmResetProgressPanel != null) confirmResetProgressPanel.SetActive(true);
    }
    public void CancelResetProgress()
    {
        if (confirmResetProgressPanel != null) confirmResetProgressPanel.SetActive(false);
    }
    public void ResetProgress()
    {
        Debug.Log("TÜM ÝLERLEME SÝLÝNÝYOR... SIFIRDAN BAÞLATILIYOR.");
        PlayerPrefs.DeleteAll();

        // Reset sonrasý baþlangýç parasý ve ayarlarý (LevelManager'daki gibi)
        PlayerPrefs.SetInt("PlayerMoney", 150);
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}