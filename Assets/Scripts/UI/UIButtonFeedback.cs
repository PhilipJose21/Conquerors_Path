using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Settings")]
    [SerializeField] private Vector3 hoverScale = new Vector3(1.05f, 1.05f, 1.05f);
    [SerializeField] private Vector3 pressScale = new Vector3(0.95f, 0.95f, 0.95f);
    [SerializeField] private float speed = 15f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    
    [Tooltip("Optional: Sound played if this button opens a major UI panel (e.g., Info Panel open whoosh)")]
    [SerializeField] private AudioClip panelOpenSound; 

    private Vector3 targetScale;
    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = hoverScale;

        if (hoverSound != null)
        {
            SoundManager.Instance?.PlayHoverSFX(hoverSound);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = pressScale;

        if (clickSound != null)
        {
            SoundManager.Instance?.PlayClickSFX(clickSound);
        }
        
        if (panelOpenSound != null)
        {
            SoundManager.Instance?.PlayClickSFX(panelOpenSound);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = eventData.dragging || !RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, eventData.position, eventData.pressEventCamera) 
            ? originalScale 
            : hoverScale;
    }

    void OnDisable()
    {
        transform.localScale = originalScale;
        targetScale = originalScale;
    }
}