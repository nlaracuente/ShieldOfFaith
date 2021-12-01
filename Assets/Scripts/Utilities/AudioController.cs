using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioController : MonoBehaviour
{
    [SerializeField] string volumeParameterName = "MasterVolume";
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Slider slider;
    [SerializeField] float multiplier = 30f;
    [SerializeField] Toggle toggle;
    bool disableToggleEvent;

    // Start is called before the first frame update
    void Awake()
    {
        slider.onValueChanged.AddListener(HandleSliderValueChanged);
        toggle.onValueChanged.AddListener(HandleToggleValueChanged);
    }

    private void HandleToggleValueChanged(bool enableSound)
    {
        if (disableToggleEvent) return;

        if (enableSound)
            SetSliderVolumeToPlayerPrefVolume();
        else
        {
            SaveCurrentVolume();
            slider.value = slider.minValue;
        }
            
    }

    private void HandleSliderValueChanged(float value)
    {
        audioMixer.SetFloat(volumeParameterName, Mathf.Log10(value) * multiplier);
        disableToggleEvent = true;
        toggle.isOn = value > slider.minValue;
        disableToggleEvent = false;
    }

    private void Start()
    {
        SetSliderVolumeToPlayerPrefVolume();
    }

    private void SetSliderVolumeToPlayerPrefVolume()
    {
        slider.value = PlayerPrefs.GetFloat(volumeParameterName, slider.value);
    }

    private void OnDisable()
    {
        SaveCurrentVolume();
    }

    private void SaveCurrentVolume()
    {
        PlayerPrefs.SetFloat(volumeParameterName, slider.value);
    }
}
