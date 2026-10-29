using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButtonTrigger : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Linkitä tähän tekstin sulamisskripti")]
    [SerializeField] private TMPSmeltingEffect targetEffect;

    void Awake()
    {
        // Jos skriptiä ei ole linkitetty Inspectorissa, etsitään se automaattisesti lapsiobjekteista
        if (targetEffect == null)
        {
            targetEffect = GetComponentInChildren<TMPSmeltingEffect>();
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (targetEffect != null) targetEffect.StartMelting();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (targetEffect != null) targetEffect.StopMelting();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetEffect != null) targetEffect.StartMelting();
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetEffect != null) targetEffect.StopMelting();
    }

    void OnDisable()
    {
        if (targetEffect != null) targetEffect.StopMelting();
    }
}
