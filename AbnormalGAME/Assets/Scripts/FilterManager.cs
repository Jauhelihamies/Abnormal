using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenFilterManager : MonoBehaviour
{
    [Header("UI-elementit")]
    [SerializeField] private TMP_Dropdown filterDropdown;

    [Header("Post-Processing asennus")]
    [SerializeField] private Volume globalVolume;

  
    private ColorAdjustments colorAdjustments;
    private SplitToning splitToning;
    private Vignette vignette;


    private const string FilterPrefsKey = "SelectedScreenFilter";

    void Start()
    {
        // Haetaan tarvittavat efektit Volumesta
        if (globalVolume.profile.TryGet<ColorAdjustments>(out var adjustments)) colorAdjustments = adjustments;
        if (globalVolume.profile.TryGet<SplitToning>(out var toning)) splitToning = toning;
        if (globalVolume.profile.TryGet<Vignette>(out var vig)) vignette = vig;

        if (filterDropdown != null)
        {

            int savedFilter = PlayerPrefs.GetInt(FilterPrefsKey, 0);


            filterDropdown.value = savedFilter;
            OnFilterChanged(savedFilter);


            filterDropdown.onValueChanged.AddListener(OnFilterChanged);
        }
    }

    public void OnFilterChanged(int index)
    {

        ResetEffects();


        switch (index)
        {
            case 0: 
                break;

            case 1: 
                if (colorAdjustments != null)
                {
                    colorAdjustments.saturation.value = -100f;
                    colorAdjustments.contrast.value = 25f;
                }
                break;

            case 2: 
                if (colorAdjustments != null)
                {
                    colorAdjustments.saturation.value = -25f;
                    colorAdjustments.colorFilter.value = new Color(0.82f, 0.70f, 0.50f);
                }
                break;

            case 3: 

                if (colorAdjustments != null)
                {
                    colorAdjustments.saturation.value = -40f;
                    colorAdjustments.contrast.value = 40f; // 
                    colorAdjustments.colorFilter.value = new Color(0.7f, 0.9f, 0.75f); 
                }


                if (splitToning != null)
                {
                    splitToning.active = true;
                    splitToning.shadows.value = new Color(0f, 0.2f, 0.4f);
                    splitToning.highlights.value = new Color(0.6f, 0.5f, 0.2f);
                }

                if (vignette != null)
                {
                    vignette.active = true;
                    vignette.intensity.value = 0.5f;
                    vignette.smoothness.value = 0.8f;
                    vignette.color.value = Color.black;
                }
                break;
        }
        PlayerPrefs.SetInt(FilterPrefsKey, index);
        PlayerPrefs.Save(); 
    }

    private void ResetEffects()
    {
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = 0f;
            colorAdjustments.contrast.value = 0f;
            colorAdjustments.colorFilter.value = Color.white;
        }
        if (splitToning != null) splitToning.active = false;
        if (vignette != null) vignette.active = false;
    }

    private void OnDestroy()
    {
        if (filterDropdown != null)
        {
            filterDropdown.onValueChanged.RemoveListener(OnFilterChanged);
        }
    }
}
