using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement; 

public class ScreenFilterManager : MonoBehaviour
{

    public static ScreenFilterManager Instance { get; private set; }

    [Header("UI-elementit (Voi olla null muissa sceneissä)")]
    [SerializeField] private TMP_Dropdown filterDropdown;


    private ColorAdjustments colorAdjustments;
    private SplitToning splitToning;
    private Vignette vignette;
    private FilmGrain filmGrain;
    private ChromaticAberration chromaticAberration;

    private const string FilterPrefsKey = "SelectedScreenFilter";

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        InitializeCurrentScene();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (filterDropdown != null)
        {
            filterDropdown.onValueChanged.RemoveListener(OnFilterChanged);
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeCurrentScene();
    }

    private void InitializeCurrentScene()
    {

        Volume globalVolume = Object.FindFirstObjectByType<Volume>();

        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out colorAdjustments);
            globalVolume.profile.TryGet(out splitToning);
            globalVolume.profile.TryGet(out vignette);
            globalVolume.profile.TryGet(out filmGrain);
            globalVolume.profile.TryGet(out chromaticAberration);
        }
        if (filterDropdown == null)
        {
            filterDropdown = Object.FindFirstObjectByType<TMP_Dropdown>();
        }

        int savedFilter = PlayerPrefs.GetInt(FilterPrefsKey, 0);

        if (filterDropdown != null)
        {
            filterDropdown.onValueChanged.RemoveListener(OnFilterChanged); 
            filterDropdown.value = savedFilter;
            filterDropdown.onValueChanged.AddListener(OnFilterChanged);
        }
        ApplyFilterEffects(savedFilter);
    }

    public void OnFilterChanged(int index)
    {
        PlayerPrefs.SetInt(FilterPrefsKey, index);
        PlayerPrefs.Save();
        ApplyFilterEffects(index);
    }

    private void ApplyFilterEffects(int index)
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
                    colorAdjustments.contrast.value = 40f;
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
                }
                break;

            case 4: 

                if (colorAdjustments != null)
                {
                    colorAdjustments.saturation.value = 15f; 
                    colorAdjustments.contrast.value = 50f;   
                    colorAdjustments.colorFilter.value = new Color(1.0f, 0.15f, 0.15f); 
                }
                if (splitToning != null)
                {
                    splitToning.active = true;
                    splitToning.shadows.value = Color.black;
                    splitToning.highlights.value = new Color(0.8f, 0.1f, 0.3f);
                }

                // Klaustrofobinen reunan pimennys
                if (vignette != null)
                {
                    vignette.active = true;
                    vignette.intensity.value = 0.6f;
                    vignette.smoothness.value = 0.9f;
                    vignette.color.value = Color.black;
                }
                if (filmGrain != null)
                {
                    filmGrain.active = true;
                    filmGrain.type.value = FilmGrainLookup.Large02; 

                    filmGrain.intensity.value = 0.8f;            
                    filmGrain.response.value = 0.5f;
                }
                if (chromaticAberration != null)
                {
                    chromaticAberration.active = true;
                    chromaticAberration.intensity.value = 0.7f; 
                }
                break;
        }
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
        if (filmGrain != null) filmGrain.active = false;
        if (chromaticAberration != null) chromaticAberration.active = false;
    }
}
