using UnityEngine;

/// <summary>
/// Gestiona toda la reproducción de audio del juego.
/// Música adaptativa según el estado del juego y SFX via EventBus.
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("Música")]
    [SerializeField] private AudioClip musicCalm;
    [SerializeField] private AudioClip musicIntense;
    [SerializeField] private AudioClip musicStorm;

    [Header("SFX")]
    [SerializeField] private AudioClip sfxNearMiss;
    [SerializeField] private AudioClip sfxDeath;
    [SerializeField] private AudioClip sfxUpgrade;
    [SerializeField] private AudioClip sfxThrust;

    [Header("Volúmenes")]
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.6f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 0.8f;
    [Range(0f, 1f)][SerializeField] private float thrustVolume = 0.3f;

    // Sources
    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioSource thrustSource;

    // Estado
    private float elapsedTime;
    private bool isPlaying;
    private bool isIntenseMusicPlaying;
    private const float IntenseMusicThreshold = 45f;

    private void Awake()
    {
        // Crear tres AudioSources en el mismo GameObject
        musicSource = CreateAudioSource("MusicSource", musicVolume, true);
        sfxSource = CreateAudioSource("SFXSource", sfxVolume, false);
        thrustSource = CreateAudioSource("ThrustSource", thrustVolume, true);
    }

    private AudioSource CreateAudioSource(string sourceName, float volume, bool loop)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.volume = volume;
        source.loop = loop;
        source.playOnAwake = false;
        return source;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<RunStartedEvent>(OnRunStarted);
        EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        EventBus.Subscribe<NearMissEvent>(OnNearMiss);
        EventBus.Subscribe<UpgradePurchasedEvent>(OnUpgradePurchased);
        EventBus.Subscribe<AsteroidStormEvent>(OnAsteroidStorm);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<RunStartedEvent>(OnRunStarted);
        EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        EventBus.Unsubscribe<NearMissEvent>(OnNearMiss);
        EventBus.Unsubscribe<UpgradePurchasedEvent>(OnUpgradePurchased);
        EventBus.Unsubscribe<AsteroidStormEvent>(OnAsteroidStorm);
    }

    private void Update()
    {
        if (!isPlaying) return;

        elapsedTime += Time.deltaTime;
        UpdateAdaptiveMusic();
    }

    // ─── Música adaptativa ────────────────────────────────────────────

    private void UpdateAdaptiveMusic()
    {
        // Cambiar a música intensa después de 45 segundos
        if (!isIntenseMusicPlaying && elapsedTime >= IntenseMusicThreshold)
        {
            if (musicIntense != null)
                CrossfadeMusic(musicIntense);

            isIntenseMusicPlaying = true;
        }
    }

    private void CrossfadeMusic(AudioClip newClip)
    {
        // Crossfade simple — detiene la actual y empieza la nueva
        // Para un crossfade suave necesitarías dos AudioSources y una corrutina
        musicSource.clip = newClip;
        musicSource.Play();
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.Play();
    }

    private void StopMusic()
    {
        musicSource.Stop();
        thrustSource.Stop();
    }

    // ─── SFX ──────────────────────────────────────────────────────────

    public void PlayThrust(bool active)
    {
        if (sfxThrust == null) return;

        if (active && !thrustSource.isPlaying)
        {
            thrustSource.clip = sfxThrust;
            thrustSource.Play();
        }
        else if (!active && thrustSource.isPlaying)
        {
            thrustSource.Stop();
        }
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // ─── Eventos ──────────────────────────────────────────────────────

    private void OnRunStarted(RunStartedEvent e)
    {
        elapsedTime = 0f;
        isPlaying = true;
        isIntenseMusicPlaying = false;

        if (musicCalm != null)
            PlayMusic(musicCalm);
    }

    private void OnPlayerDied(PlayerDiedEvent e)
    {
        // Solo reaccionar al evento real (con datos)
        if (e.finalScore == 0 && e.survivalTime == 0f) return;

        isPlaying = false;
        StopMusic();
        PlaySFX(sfxDeath);
    }

    private void OnNearMiss(NearMissEvent e)
    {
        PlaySFX(sfxNearMiss);
    }

    private void OnUpgradePurchased(UpgradePurchasedEvent e)
    {
        PlaySFX(sfxUpgrade);
    }

    private void OnAsteroidStorm(AsteroidStormEvent e)
    {
        // Durante el storm cambiar temporalmente a música de storm si existe
        if (musicStorm != null)
            CrossfadeMusic(musicStorm);
    }
}
