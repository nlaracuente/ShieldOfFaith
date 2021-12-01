using UnityEngine;

[CreateAssetMenu(fileName = "SoundEffect", menuName = "Audio/SoundFX")]
public class SoundEffect : ScriptableObject
{
    [Tooltip("Prevents multiple instances of the sound to play at the same time")]
    public bool preventMultiple = false;

    [Tooltip("Turns this ON if you want the sound to play continiously")]
    public bool loop = false;

    [Tooltip("Single clip to play")]
    public AudioClip clip;

    [Tooltip("Random clips to play")]
    public AudioClip[] clips;

    [Range(0.1f, 1f), Tooltip("Volume to play the audio at")]
    public float volume = 1f;

    [Range(0.1f, 1f), Tooltip("Pitch to play the audio at")]
    public float pitch = 1f;    

    [Tooltip("Minimum/Maximum values to randomize the pitch at. zero or below will be ignored")]
    public MinMaxFloat randomPitch = new MinMaxFloat(0f, 0f);
}

