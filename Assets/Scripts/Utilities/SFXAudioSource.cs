using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFXAudioSource : MonoBehaviour
{
    AudioSource source;
    public AudioSource Source
    {
        get
        {
            if (source == null)
                source = GetComponent<AudioSource>();
            return source;
        }
    }

    bool isPaused = false;
    public void OnSoundToggled(bool enabled)
    {
        if (enabled && isPaused)
            Source.Play();

        if (!enabled && source.isPlaying)
        {
            isPaused = true;
            Source.Pause();
        }
    }

    public void StopPlaying() => Source.Stop();
    public void ForceRelease()
    {
        isPaused = false;
        Source.Stop();
        StopCoroutine(ReleaseAudioSourceRoutine());
    }

    public AudioSource Play(SoundEffect sfx)
    {
        if (sfx == null)
            return null;

        AudioClip clip = sfx.clip;
        if (sfx.clips != null && sfx.clips.Length > 0)
            clip = AudioManager.GetRandomClip(sfx.clips);

        // Strange...no clip given
        if (clip == null)
            return null;

        var pitch = sfx.pitch;
        if (sfx.randomPitch.min > 0 && sfx.randomPitch.max > 0)
            pitch = Random.Range(sfx.randomPitch.min, sfx.randomPitch.max);

        Source.clip = clip;
        Source.pitch = pitch;
        Source.loop = sfx.loop;
        Source.volume = sfx.volume;
        Source.Play();

        StartCoroutine(ReleaseAudioSourceRoutine());
        return Source;
    }

    IEnumerator ReleaseAudioSourceRoutine()
    {
        yield return new WaitForEndOfFrame();

        var release = false;
        while (!release)
        {
            // Audio source is paused so we don't want to release it
            if(isPaused)
            {
                while(isPaused)
                    yield return new WaitForEndOfFrame();
                continue;
            }

            // Audio source no longer playing - time to release it
            if (!Source.isPlaying)
                release = true;

            yield return new WaitForEndOfFrame();
        }   

        AudioManager.instance.Release(this);
    }
}
