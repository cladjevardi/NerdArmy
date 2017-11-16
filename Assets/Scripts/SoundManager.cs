using UnityEngine;

public class SoundManager : MonoBehaviour
{
    /// <summary>The audio source reference which will play sound effects.</summary>
    public AudioSource soundEffectSource;

    /// <summary>The audio source reference which will play music</summary>
    public AudioSource musicSource;

    /// <summary>The lowest a sound effect will be randomly pitched.</summary>
    public float lowPitchRange = .95f;

    /// <summary>The highest a sound effect will be randomly pitched.</summary>
    public float highPitchRange = 1.05f;

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