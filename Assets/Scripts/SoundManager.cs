using UnityEngine;

public class SoundManager : MonoBehaviour
{
    /// <summary>
    /// The static instance of the sound manager. Allows other scripts to
    /// reference and make calls for music and sound effects.
    /// </summary>
    public static SoundManager instance = null;

    /// <summary>The audio source reference which will play sound effects.</summary>
    public AudioSource soundEffectSource;

    /// <summary>The audio source reference which will play music</summary>
    public AudioSource musicSource;

    /// <summary>The lowest a sound effect will be randomly pitched.</summary>
    public float lowPitchRange = .95f;

    /// <summary>The highest a sound effect will be randomly pitched.</summary>
    public float highPitchRange = 1.05f;

    void Awake()
    {
        // Keep track of our singleton instance
        if (instance == null)
            instance = this;

        // There can only be one sound manager instance.
        else if (instance != this)
            Destroy(gameObject);

        // Reloading scene will not trigger the sound manager to be destroyed.
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>Play a single sound clip.</summary>
    /// <param name="clip">The sound effect clip to play.</param>
    public void PlaySingleSoundEffect(AudioClip clip, bool randomizePitch = true)
    {
        soundEffectSource.clip = clip;

        // Choose a random pitch to play back our clip at between our
        // high and low pitch ranges.
        if (randomizePitch)
        {
            soundEffectSource.pitch = Random.Range(lowPitchRange, highPitchRange);
        }

        // Play the sound effect.
        soundEffectSource.Play();
    }

    /// <summary>Play a random sound clip from a list of provided sound clips.</summary>
    /// <param name="clips">An array of possible sounds to play.</param>
    public void PlayRandomSoundEffect(params AudioClip[] clips)
    {
        // Generate a random number between 0 and the length of our array of clips passed in.
        PlaySingleSoundEffect(clips[Random.Range(0, clips.Length)], true);
    }
}