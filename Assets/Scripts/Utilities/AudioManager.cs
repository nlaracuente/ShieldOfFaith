using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Audio;
using UnityEngine.Pool;

public enum MixerType
{
    MasterAudio, // Handles all volume
    MusicAudio,  // Handles music volume
    SFXAudio,    // Handles sound effects volume
}

/// <summary>
/// Handles the play back of all types of audio (music/sfx)
/// We use an AudioMixer to control the audio output based on
/// current audio settings. 
/// 
/// Sound effects are handled via a pool of audio sources to allocate 
/// resources to play sounds when needed while also limiting how many
/// effects can be played simultaniously.
/// 
/// Currently only a single music clip can be played at once.
/// Meaning, you can only change the track.
/// To do multiple tracks would require multiple mixers which are you are free to do
/// but it is beyond the scope of this package (at least currently)
/// 
/// For the code that handles audio settings changes <see cref="AudioSettings"/>
/// 
/// TODO: Since we are using SoundEffects scriptable objects and ObjectPool now we should clean this class up from the old way of doing things
/// </summary>
public class AudioManager : Singleton<AudioManager>
{
    /// <summary>
    /// When converting the slider value into a log value we need to multiply it 
    /// to get a volume between -80 and 0. Assuming the min slider value is set to 
    /// 0.0001 and the max value is set to 1, multiplying by 20f seems to do the trick
    /// </summary>
    const float DEFAULT_MULTIPLIER = 20f;

    /// <summary>
    /// The name audio mixer group's parent name"
    /// </summary>
    const string AUDIO_MIXER_MASTER_NAME = "Master";

    /// <summary>
    /// The name audio mixer that controls the music
    /// </summary>
    const string AUDIO_MIXER_MUSIC_NAME = "Music";

    /// <summary>
    /// The name audio mixer that controls the sound effects
    /// </summary>
    const string AUDIO_MIXER_SFX_NAME = "SFX";

    [SerializeField, Tooltip("SFX to preview volume change")]
    AudioClip sfxPreviewClip;

    [SerializeField, Tooltip("Silences the playback of the music")]
    bool muteMusic;

    [SerializeField, Tooltip("Silences the playback of the sfx")]
    bool muteSfx;

    [SerializeField, Tooltip("Main audio mixer that handles all audio output")]
    AudioMixer mixer;    

    [SerializeField, Tooltip("AudioMixer parameter name that handles the master volume")]
    string masterVolumeParameterName = "MasterVolume";

    [SerializeField, Tooltip("AudioMixer parameter name that handles the music volume")]
    string musicVolumeParameterName = "MusicVolume";

    [SerializeField, Tooltip("AudioMixer parameter name that handles the sfx volume")]
    string sfxVolumeParameterName = "SFXVolume";

    [SerializeField, Range(0f, 1f)]
    float defaultMasterVolume = 1f;

    [SerializeField, Range(0f, 1f)]
    float defaultMusicVolume = .25f;

    [SerializeField, Range(0f, 1f)]
    float defaultSFXVolume = .5f;

    [SerializeField]
    [Tooltip("How many audio sources to create to handle SFX. " +
             "This limits how many total sounds can be played at one time")]
    int sfxPoolSize = 30;

    List<AudioSource> sfxSources;
    List<AudioSource> SFXSources
    {
        get
        {
            if (sfxSources == null || sfxSources.Count != sfxPoolSize)
                sfxSources = CreateAudioSources(AUDIO_MIXER_SFX_NAME, sfxPoolSize);
            return sfxSources;
        }
    }

    AudioSource musicSource;
    AudioSource MusicSource
    {
        get
        {
            if (musicSource == null)
            {
                var sources = CreateAudioSources(AUDIO_MIXER_MUSIC_NAME, 1);
                musicSource = sources[0];
                musicSource.loop = true;
            }

            return musicSource;
        }
    }

    /// <summary>
    /// Audio sources holders
    /// </summary>
    Dictionary<string, GameObject> sourceGOs;
    Dictionary<string, GameObject> SourceGOs
    {
        get
        {
            // Not defined
            if (sourceGOs == null)
                BuildAudioSourcesDictionary();

            return sourceGOs;
        }
    }

    ObjectPool<SFXAudioSource> srcPool;

    /// <summary>
    /// Actively playing sources when a call to pause them is triggered
    /// </summary>
    List<AudioSource> activeSFXsources;
    List<AudioSource> ActiveSFXsources
    {
        set { activeSFXsources = value; }
        get
        {
            if (activeSFXsources == null)
                activeSFXsources = new List<AudioSource>();
            return activeSFXsources;
        }
    }

    public delegate void TogglePlayback(bool enabled);
    public TogglePlayback TogglePlaybackDelegates;

    public delegate void StopSound();
    public StopSound StopSoundDelegates;

    void Start()
    {
        if(gameObject != null)
        {
            // Set default volumes
            SetMasterVolume(defaultMasterVolume);
            SetMusicVolume(defaultMusicVolume);
            SetSFXVolume(defaultSFXVolume);

            srcPool = new ObjectPool<SFXAudioSource>(CreateSFXSource);
        }
    }

    SFXAudioSource CreateSFXSource()
    {
        var mixerGroupName = AUDIO_MIXER_SFX_NAME;

        var go = new GameObject(mixerGroupName);
        go.transform.SetParent(transform);
        SourceGOs[mixerGroupName] = go;

        // Mixer group to handle volume
        var mixerGroup = mixer.FindMatchingGroups(AUDIO_MIXER_MASTER_NAME)
                              .Where(g => g.name == mixerGroupName).First();

        AudioSource src; // Optimization to resuse resource
        src = go.AddComponent<AudioSource>();
        src.outputAudioMixerGroup = mixerGroup;

        var sfx = go.AddComponent<SFXAudioSource>();

        TogglePlaybackDelegates += sfx.OnSoundToggled;
        StopSoundDelegates += sfx.StopPlaying;

        return sfx;
    }

    public void Release(SFXAudioSource src) => srcPool.Release(src);

    void BuildAudioSourcesDictionary()
    {
        sourceGOs = new Dictionary<string, GameObject>();
        string[] mixerNames = new string[] { AUDIO_MIXER_MUSIC_NAME, AUDIO_MIXER_SFX_NAME };
        foreach (var mixerName in mixerNames)
            CreateAudioSourceGameObject(mixerName);
    }

    /// <summary>
    /// Note to self "Do not use SourceGOs or you will create a recurrence loop"
    /// </summary>
    /// <param name="mixerName"></param>
    void CreateAudioSourceGameObject(string mixerName)
    {
        // Already exists
        if (sourceGOs.ContainsKey(mixerName) && mixerName != null)
            return;

        var go = new GameObject(mixerName);
        go.transform.SetParent(transform);
        sourceGOs[mixerName] = go;
    }

    public void SetMasterVolume(float volume) => SetVolume(masterVolumeParameterName, volume);
    public void SetMusicVolume(float volume) => SetVolume(musicVolumeParameterName, volume);
    public void SetSFXVolume(float volume) => SetVolume(sfxVolumeParameterName, volume);
    void SetVolume(string name, float volume, float multiplier = DEFAULT_MULTIPLIER)
    {
        // Anything above "0" increased the sound where it starts to clip
        // While -80f is the lowest value we can set something to
        var value = Mathf.Clamp(Mathf.Log10(volume) * multiplier, -80f, 0f);
        if(mixer != null)
            mixer.SetFloat(name, value);
    }

    public void SetVolume(MixerType type, float volume)
    {
        switch (type)
        {
            case MixerType.MasterAudio:
                defaultMasterVolume = volume;
                SetMasterVolume(volume);
                break;
            case MixerType.MusicAudio:
                defaultMusicVolume = volume;
                SetMusicVolume(volume);
                break;
            case MixerType.SFXAudio:
                defaultSFXVolume = volume;
                SetSFXVolume(volume);
                break;
        }
    }

    /// <summary>
    /// Plays a clip for the given type, if one exists, to preview what the volume
    /// for that specific type of audio will sound like with given change
    /// </summary>
    /// <param name="type"></param>
    /// <param name="volume"></param>
    public void SetAndPreviewVolumeChange(MixerType type, float volume)
    {
        switch (type)
        {
            case MixerType.SFXAudio:
                PlayClip(sfxPreviewClip, true);
                break;
        }

        SetVolume(type, volume);
    }

    public float GetDefaultVolume(MixerType type)
    {
        float volume = 0f;
        switch (type)
        {
            case MixerType.MasterAudio:
                volume = defaultMasterVolume;
                break;
            case MixerType.MusicAudio:
                volume = defaultMusicVolume;
                break;
            case MixerType.SFXAudio:
                volume = defaultSFXVolume;
                break;
        }

        return volume;
    }

    List<AudioSource> CreateAudioSources(string mixerGroupName, int poolSize)
    {
        List<AudioSource> sources = new List<AudioSource>();

        // Does it already exist?
        GameObject go = SourceGOs[mixerGroupName] ?? null;
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.name == mixerGroupName)
            {
                go = child.gameObject;
                sources = go.GetComponents<AudioSource>().ToList();
                break;
            }
        }

        // Use existing one or create a new one
        if (go == null)
        {
            go = new GameObject(mixerGroupName);
            go.transform.SetParent(transform);
            SourceGOs[mixerGroupName] = go;
        }

        // Mixer group to handle volume
        var mixerGroup = mixer.FindMatchingGroups(AUDIO_MIXER_MASTER_NAME)
                              .Where(g => g.name == mixerGroupName).First();

        AudioSource src; // Optimization to resuse resource
        var total = poolSize - sources.Count; // Adds missing sources or creates the entire pool
        for (int i = 0; i < total; i++)
        {
            src = go.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = mixerGroup;
            sources.Add(src);
        }

        return sources;
    }

    /// <summary>
    /// Sets the current track to the given clip and plays it making it loop
    /// Note: if you have a "jingle" it is better to treat it as a sfx and play it that way
    /// </summary>
    /// <param name="clip"></param>
    public void PlayMusic(AudioClip clip)
    {
        // Already playing
        if (MusicSource.clip == clip)
        {
            if(!MusicSource.isPlaying)
                MusicSource.Play();
            return;
        }           

        MusicSource.Stop();
        MusicSource.loop = true; // redundant but safe
        MusicSource.clip = clip;
        
        if(!muteMusic)
            MusicSource.Play();
    }

    /// <summary>
    /// Returns the first available source that is not actively playing a sound
    /// The clip might be set to "loop" and currently "paused" therefore should 
    /// be excluded from 
    /// </summary>
    /// <returns></returns>
    AudioSource GetAvailableSource()
    {
        //return srcPool.Get();
        return SFXSources.Where(s => !s.isPlaying && !s.loop).FirstOrDefault();
    }

    /// <summary>
    /// Chooses a random clip to play from the given collection of clips
    /// </summary>
    /// <param name="clips"></param>
    /// <param name="ignoreIfPlaying"></param>
    /// <returns></returns>
    public AudioSource PlayClip(AudioClip[] clips, bool ignoreIfPlaying = false, float pitch = 1f)
    {
        var index = Random.Range(0, clips.Length);
        var clip = clips[index];
        return PlayClip(clip, ignoreIfPlaying, pitch);
    }

    public AudioSource PlayClip(AudioClip[] clips, float minPitch, float maxPitch, bool ignoreIfPlaying = false)
    {
        float pitch = Random.Range(minPitch, maxPitch);
        AudioClip clip = GetRandomClip(clips);
        return PlayClip(clip, ignoreIfPlaying, pitch);
    }

    public static AudioClip GetRandomClip(AudioClip[] clips)
    {
        var index = Random.Range(0, clips.Length);
        var clip = clips[index];
        return clip;
    }

    /// <summary>
    /// Plays the given clip if a source is available
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="ignoreIfPlaying"></param>
    /// <returns></returns>
    public AudioSource PlayClip(AudioClip clip, bool ignoreIfPlaying = false, float pitch = 1f)
    {
        AudioSource src = null;

        // Don't allow the clip to play more than once at a time
        if(ignoreIfPlaying)
        {
            src = SFXSources.Where(s => s.clip == clip && s.isPlaying).FirstOrDefault();
            if (src != null)
                return src; // already playing
        }        

        src = GetAvailableSource();
        if (src == null || clip == null)
            return null;

        src.clip = clip;
        src.pitch = pitch;
        src.Play();

        return src;
    }

    public AudioSource PlayClip(AudioClip clip, float minPitch, float maxPitch, bool ignoreIfPlaying = false)
    {
        float pitch = Random.Range(minPitch, maxPitch);
        return PlayClip(clip, ignoreIfPlaying, pitch);
    }

    public AudioSource Play(SoundEffect sfx)
    {
        //AudioSource src = null;

        //// Need an object to work with
        //if (sfx == null || muteSfx)
        //    return src;

        //AudioClip clip = sfx.clip;
        //if (sfx.clips != null && sfx.clips.Length > 0)
        //    clip = GetRandomClip(sfx.clips);

        //// Strange...no clip given
        //if (clip == null)
        //    return src;

        //// Don't allow the clip to play more than once at a time
        //if (sfx.preventMultiple)
        //{
        //    src = SFXSources.Where(s => s.clip == clip && s.isPlaying).FirstOrDefault();
        //    if (src != null)
        //        return src; // already playing
        //}

        var sfxSource = srcPool.Get();

        // Get next available source
        //src = sfxSource.Source;

        //// None are available
        //if (src == null)
        //    return src;

        //var pitch = sfx.pitch;
        //if (sfx.randomPitch.min > 0 && sfx.randomPitch.max > 0)
        //    pitch = Random.Range(sfx.randomPitch.min, sfx.randomPitch.max);

        //src.clip = clip;
        //src.pitch = pitch;
        //src.loop = sfx.loop;
        //src.volume = sfx.volume;
        //src.Play();

        sfxSource.Play(sfx);

        return sfxSource.Source;
    }

    /// <summary>
    /// Plays and loops the given clip
    /// Note: 
    ///     You are responsible for calling <see cref="StopLoopingClip"/> to stop this clip from looping
    ///     or calling <see cref="StopSFXs"/> to stop all sounds otherwise the audio will play until the GameObject's is destroyed
    ///     and you will run out of available audio sources if all sounds currently playing are looping.
    /// </summary>
    /// <param name="clip"></param>
    /// <returns></returns>
    public AudioSource PlayLoopingClip(AudioClip clip)
    {
        var src = PlayClip(clip);
        src.loop = true;
        return src;
    }

    /// <summary>
    /// Updates the given audio source to no longer loop
    /// Unless told to stop immedeatly, the sound will play until the end before stopping to play
    /// </summary>
    /// <param name="src"></param>
    /// <param name="stopImmedeatly"></param>
    public void StopLoopingClip(AudioSource src, bool stopImmedeatly = false)
    {
        src.loop = false;
        if (stopImmedeatly)
            src.Stop();
    }

    /// <summary>
    /// Force stops all sounds and makes looping sounds no longer loop
    /// </summary>
    public void StopSFXs()
    {
        StopSoundDelegates?.Invoke();
        SFXSources.ForEach(s => { s.Stop(); s.loop = false; });

        foreach (var sfx in FindObjectsOfType<SFXAudioSource>())
            sfx.ForceRelease();
    }
   
    public bool PauseSounds { get; protected set; }
    public void PauseSFXs()
    {
        PauseSounds = true;
        TogglePlaybackDelegates?.Invoke(false);
        ActiveSFXsources = SFXSources.Where(s => s.isPlaying).ToList();
        ActiveSFXsources.ForEach(s => s.Pause());       
    }

    public void ResumeSFXs()
    {
        PauseSounds = false;
        TogglePlaybackDelegates?.Invoke(true);
        ActiveSFXsources.ForEach(s => s.Play());
    }

    public void PauseMusic() => MusicSource.Pause();
    public void ResumeMusic() => MusicSource.Play();

    public void PauseAudio()
    {
        PauseSFXs();
        PauseMusic();
    }

    public void ResumeAudio()
    {
        ResumeSFXs();
        ResumeMusic();
    }
}
