using UnityEngine;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    [SerializeField] MixerType type;
    [SerializeField] Slider slider;
    [SerializeField] Toggle toggle;
    bool disableToggleEvent;

    float valueBeforeMute;

    string PreferenceName
    {
        get
        {
            var n = $"{Application.productName}_{type}";
            return n;
        }
    }

    /// <summary>
    /// It is crucial this runs on start to allow time for the AudioManager
    /// to be fully built and have access to modify the mixes
    /// 
    /// Note: Yes, modifying the project's settings so that the AudioManager
    ///       runs before the VolumeControl is probably the safest option but
    ///       I did not want to complicate project setups to consume this package
    /// </summary>
    void Awake()
    {
        // Our math will not work properly unless we have these min/max values
        slider.minValue = 0.0001f;
        slider.maxValue = 1;

        // Update the slider's value prior to the listeners so that we don't 
        // trigger the listener when we first build the slider
        SetSliderVolumeToPlayerPrefVolume();

        slider.onValueChanged.AddListener(HandleSliderValueChanged);
        toggle.onValueChanged.AddListener(HandleToggleValueChanged);
    }

    /// <summary>
    /// Ensures the slider matches either the valued saved on disabled 
    /// or the default value for the specific mixer this controls
    /// </summary>
    public void SetSliderVolumeToPlayerPrefVolume()
    {
        slider.value = AudioManager.instance.GetDefaultVolume(type);
        AudioManager.instance.SetVolume(type, slider.value);
    }

    public void SetSliderVolumeToPlayerPrefVolume(float volume)
    {
        slider.value = volume;
        AudioManager.instance.SetVolume(type, slider.value);
    }

    void HandleToggleValueChanged(bool enableSound)
    {
        if (disableToggleEvent) return;

        if (enableSound)
            SetSliderVolumeToPlayerPrefVolume(valueBeforeMute);
        else
        {
            valueBeforeMute = slider.value;
            slider.value = slider.minValue;
        }
    }

    /// <summary>
    /// Changes the volume to match the slider's position
    /// </summary>
    /// <param name="value"></param>
    void HandleSliderValueChanged(float value)
    {
        AudioManager.instance.SetAndPreviewVolumeChange(type, value);

        // Toggles audio on/off but prevents the listerner from doing anything
        disableToggleEvent = true;
        toggle.isOn = value > slider.minValue;
        disableToggleEvent = false;
    }

    ///// <summary>
    ///// Store the current volume as a user preference
    ///// </summary>
    //void SaveCurrentVolume() => PlayerPrefs.SetFloat(PreferenceName, slider.value);

    ///// <summary>
    ///// Store the current volume for when we resume
    ///// </summary>
    //void OnDisable() => SaveCurrentVolume();
}
