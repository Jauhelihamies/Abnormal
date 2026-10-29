using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class MenuButtonJuicer : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Animaatioasetukset")]
    [SerializeField] private float targetScale = 1.18f;    // Kuinka paljon teksti suurenee (1.0 = normaali)
    [SerializeField] private float animationSpeed = 8f;   // Kuinka nopeasti suureneminen tapahtuu
    [SerializeField] private Color selectedColor = new Color(0.8f, 0.2f, 0.1f); // Teemaan sopiva korostusväri

    private Vector3 originalScale;
    private Color originalColor;
    private bool isTargeted = false;

    // Komponenttiviittaukset (koodi etsii nämä automaattisesti)
    private Transform textTransform;
    private Graphic textGraphic; // Toimii sekä Legacy Textille että TextMeshProlle

    void Awake()
    {
        // Etsitään tekstikomponentti lapsiobjekteista
        textGraphic = GetComponentInChildren<Graphic>();

        if (textGraphic != null)
        {
            textTransform = textGraphic.transform;
            originalScale = textTransform.localScale;
            originalColor = textGraphic.color;
        }
        else
        {
            // Jos teksti on samassa objektissa kuin painike
            textGraphic = GetComponent<Graphic>();
            if (textGraphic != null)
            {
                textTransform = transform;
                originalScale = textTransform.localScale;
                originalColor = textGraphic.color;
            }
        }
    }

    void Update()
    {
        if (textTransform == null || textGraphic == null) return;

        // Laskettaan tavoitekoko ja -väri sen mukaan, onko painike valittu
        Vector3 currentTargetScale = isTargeted ? originalScale * targetScale : originalScale;
        Color currentTargetColor = isTargeted ? selectedColor : originalColor;

        // Pehmeä interpolointi (Lerp) tekee animaatiosta sulavan ruudunpäivitysnopeudesta riippumatta
        textTransform.localScale = Vector3.Lerp(textTransform.localScale, currentTargetScale, Time.deltaTime * animationSpeed);
        textGraphic.color = Color.Lerp(textGraphic.color, currentTargetColor, Time.deltaTime * animationSpeed);
    }

    // --- Ohjaimen/Näppäimistön valinnat ---
    public void OnSelect(BaseEventData eventData)
    {
        isTargeted = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isTargeted = false;
    }

    // --- Hiiren valinnat (Hover) ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        isTargeted = true;
        // Siirretään myös ohjaimen fokus tähän, jos hiirellä klikataan/liikutetaan
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isTargeted = false;
    }

    // Varmistetaan, että koko palautuu heti jos objekti sammutetaan kesken animaation
    void OnDisable()
    {
        isTargeted = false;
        if (textTransform != null) textTransform.localScale = originalScale;
        if (textGraphic != null) textGraphic.color = originalColor;
    }
}

