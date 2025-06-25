using UnityEngine;
using System.Collections.Generic;

public class SFXManager :  MonoBehaviour
{
    public static SFXManager Instance;
    [System.Serializable]
    public class SFXEntry
    {
        public string clipName; // e.g., "knife", "gun", "reload"
        public AudioClip clip;
    }

    public AudioSource audioSource;
    public List<SFXEntry> soundClips;

    private Dictionary<string, AudioClip> clipDict;

    void Awake()
    {
        if (Instance != this)
        {
           Instance = this;
        }
        clipDict = new Dictionary<string, AudioClip>();
        foreach (var entry in soundClips)
        {
            if (!clipDict.ContainsKey(entry.clipName))
                clipDict.Add(entry.clipName, entry.clip);
        }

    }

    /// <summary>
    /// Call this from anywhere: SFXManager.Instance.PlaySFX("knife");
    /// </summary>
    public void PlaySFX(string name, float volume = 1f)
    {
        if (clipDict == null)
        {
            Debug.LogError("SFXManager: clipDict is not initialized.");
            return;
        }

        if (!clipDict.TryGetValue(name, out AudioClip clip) || clip == null)
        {
            Debug.LogWarning($"SFXManager: No AudioClip found for key '{name}'.");
            return;
        }

        if (audioSource == null)
        {
            Debug.LogError("SFXManager: AudioSource is not assigned.");
            return;
        }

        audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

}
