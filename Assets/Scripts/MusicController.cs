using UnityEngine;
using UnityEngine.UI;

public class MusicButtonController : MonoBehaviour
{
    [Header("Görsel Ayarlar")]
    public Image buttonImage;       // Butonun üzerindeki Ýkon (Image bileþeni)
    public Sprite musicOnSprite;    // Müzik AÇIK ikonu (Ses dalgalý)
    public Sprite musicOffSprite;   // Müzik KAPALI ikonu (Çarpý iþaretli)

    private Button btn;

    void Start()
    {
        btn = GetComponent<Button>();

        // Butona týklandýðýnda ne yapacaðýný kodla baðlýyoruz.
        // Bu sayede Inspector'daki referans kaybý sorununu çözüyoruz.
        btn.onClick.AddListener(OnButtonClicked);

        // Oyun baþladýðýnda ikon doðru mu diye kontrol et
        UpdateIcon();
    }

    void OnButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            // Sesi kapat/aç
            AudioManager.Instance.ToggleBackgroundMusicButton();

            // Týklama sesi çal (Opsiyonel)
            AudioManager.Instance.PlayClick();

            // Ýkonu güncelle
            UpdateIcon();
        }
    }

    void UpdateIcon()
    {
        // AudioManager'a sor: Ses kapalý mý?
        if (AudioManager.Instance != null && buttonImage != null)
        {
            bool isMuted = AudioManager.Instance.IsMusicMuted;

            // Eðer sessizdeyse "Kapalý", deðilse "Açýk" ikonunu koy
            buttonImage.sprite = isMuted ? musicOffSprite : musicOnSprite;
        }
    }
}