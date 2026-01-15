using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Ses Klipleri")]
    public AudioClip bgmMusic;
    public AudioClip shootSound;
    public AudioClip hitSound;
    public AudioClip explosionSound;
    public AudioClip mergeSound;
    public AudioClip coinSound;
    public AudioClip clickSound;
    public AudioClip winSound;
    public AudioClip loseSound;

    [Header("Kaynaklar")]
    private AudioSource musicSource;
    private AudioSource sfxSource;

    [Header("Genel Ses Ayarlarý (Master)")]
    [Range(0f, 1f)] public float masterMusicVolume = 0.8f;
    [Range(0f, 1f)] public float masterSFXVolume = 1.0f;

    [Header("Özel Efekt Seviyeleri")]
    [Range(0f, 1f)] public float volShoot = 0.3f;
    [Range(0f, 1f)] public float volHit = 0.6f;
    [Range(0f, 1f)] public float volExplosion = 0.9f;
    [Range(0f, 1f)] public float volMerge = 0.7f;
    [Range(0f, 1f)] public float volCoin = 0.5f;
    [Range(0f, 1f)] public float volClick = 0.4f;
    [Range(0f, 1f)] public float volWin = 0.8f;
    [Range(0f, 1f)] public float volLose = 0.8f;

    // UI için özellikler
    public bool IsMusicMuted
    {
        get { return musicSource != null && musicSource.mute; }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PlayMusic(bgmMusic);
    }

    void InitializeAudioSources()
    {
        GameObject musicObj = new GameObject("MusicSource");
        musicObj.transform.parent = transform;
        musicSource = musicObj.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = masterMusicVolume;

        GameObject sfxObj = new GameObject("SFXSource");
        sfxObj.transform.parent = transform;
        sfxSource = sfxObj.AddComponent<AudioSource>();
        sfxSource.volume = masterSFXVolume;
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.volume = masterMusicVolume;
        musicSource.Play();
    }

    public void ToggleBackgroundMusicButton()
    {
        if (musicSource != null)
        {
            musicSource.mute = !musicSource.mute;
        }
    }

    // --- YENÝ EKLENEN: SADECE EFEKTLERÝ KAPATIP AÇAN FONKSÝYON ---
    // PauseManager bunu çaðýracak.
    public void SetSFXState(bool isOn)
    {
        if (sfxSource != null)
        {
            // isOn = true ise (Ses Açýk), mute = false olmalý.
            // isOn = false ise (Ses Kapalý), mute = true olmalý.
            sfxSource.mute = !isOn;
        }
    }
    // -------------------------------------------------------------

    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1.0f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volumeMultiplier);
    }

    public void PlayShoot() => PlaySFX(shootSound, volShoot);
    public void PlayHit() => PlaySFX(hitSound, volHit);
    public void PlayExplosion() => PlaySFX(explosionSound, volExplosion);
    public void PlayMerge() => PlaySFX(mergeSound, volMerge);
    public void PlayCoin() => PlaySFX(coinSound, volCoin);
    public void PlayClick() => PlaySFX(clickSound, volClick);
    public void PlayWin() => PlaySFX(winSound, volWin);
    public void PlayLose() => PlaySFX(loseSound, volLose);
}