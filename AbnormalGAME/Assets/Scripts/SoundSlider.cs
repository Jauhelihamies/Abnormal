using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public AudioMixer audioMixer;
    private Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
        slider.minValue = 0.0001f; 
        slider.maxValue = 1f;
        slider.value = PlayerPrefs.GetFloat("MasterVolume", 0.75f);

        slider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float sliderValue)
    {

        float dB = Mathf.Log10(sliderValue) * 20;
        audioMixer.SetFloat("MasterVol", dB);

        PlayerPrefs.SetFloat("MasterVolume", sliderValue); 
    }
}
