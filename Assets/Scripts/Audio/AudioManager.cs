using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central audio manager. Handles music playback and pooled SFX.
/// Call AudioManager.GetOrCreate() from GameManager.Start() to ensure it exists.
/// Assign clips in the Inspector after re-running the Game Setup, or drop in AudioClips manually.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("SFX")]
    [SerializeField] private AudioClip[] sfxClips;
    [SerializeField] private string[] sfxNames;
    [SerializeField] private int poolSize = 12;

    [Header("Music")]
    [SerializeField] private AudioClip musicTrack;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.45f;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.8f;

    private AudioSource musicSource;
    private AudioSource[] sfxPool;
    private int poolIndex;
    private readonly Dictionary<string, AudioClip> sfxLookup = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        BuildSFXLookup();
        CreateAudioPool();
        SetupMusic();
    }

    /// <summary>Ensures an instance exists, creating one if needed.</summary>
    public static AudioManager GetOrCreate()
    {
        if (Instance == null)
        {
            var go = new GameObject("[AudioManager]");
            Instance = go.AddComponent<AudioManager>();
        }
        return Instance;
    }

    private void BuildSFXLookup()
    {
        if (sfxClips == null || sfxNames == null) return;
        int count = Mathf.Min(sfxClips.Length, sfxNames.Length);
        for (int i = 0; i < count; i++)
        {
            if (sfxClips[i] != null && !string.IsNullOrEmpty(sfxNames[i]))
                sfxLookup[sfxNames[i]] = sfxClips[i];
        }
    }

    private void CreateAudioPool()
    {
        sfxPool = new AudioSource[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            var go = new GameObject("SFX_" + i);
            go.transform.SetParent(transform);
            sfxPool[i] = go.AddComponent<AudioSource>();
            sfxPool[i].volume = sfxVolume;
        }
    }

    private void SetupMusic()
    {
        var go = new GameObject("Music");
        go.transform.SetParent(transform);
        musicSource = go.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;

        if (musicTrack != null)
        {
            musicSource.clip = musicTrack;
            musicSource.Play();
        }
    }

    /// <summary>Play a sound effect by name. Silently does nothing if name not found.</summary>
    public static void PlaySFX(string name, float volumeScale = 1f)
    {
        if (Instance == null) return;
        if (!Instance.sfxLookup.TryGetValue(name, out AudioClip clip)) return;

        AudioSource src = Instance.sfxPool[Instance.poolIndex % Instance.sfxPool.Length];
        Instance.poolIndex++;
        src.volume = Instance.sfxVolume * volumeScale;
        src.clip = clip;
        src.Play();
    }

    /// <summary>Play a sound effect at a world position.</summary>
    public static void PlaySFXAt(string name, Vector3 position, float volumeScale = 1f)
    {
        if (Instance == null) return;
        if (!Instance.sfxLookup.TryGetValue(name, out AudioClip clip)) return;
        AudioSource.PlayClipAtPoint(clip, position, Instance.sfxVolume * volumeScale);
    }

    /// <summary>Set music volume (0-1).</summary>
    public static void SetMusicVolume(float volume)
    {
        if (Instance?.musicSource != null)
            Instance.musicSource.volume = volume;
    }
}
