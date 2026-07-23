using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // Required for Pointer event interfaces
using System.Collections;

public class UnitButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public BuildingData buildingData; 
    public UnitSO unitData; 
    public Image unitIconRenderer; 
    public TextMeshProUGUI unitCostText, unitAmountText; 
    public GameObject fadedOverlay; 
    private string unitName; 

    public int unitCount = 1; 

    [Header("Audio SFX Clips")]
    [SerializeField] private AudioClip hoverSFX;
    [SerializeField] private AudioClip clickSFX;

    [Header("Hover & Click Animation Settings")]
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float clickScale = 0.92f;
    [SerializeField] private float animDuration = 0.1f;

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    void Start()
    {
        originalScale = transform.localScale;

        if (unitData != null) 
        {
            if (unitData.unitIcon != null) 
            {
                unitIconRenderer.sprite = unitData.unitIcon; 
            }
            unitName = unitData.unitName; 
            if (unitCostText != null && buildingData != null) unitCostText.text = buildingData.reinforcementCost.ToString(); 
            if (unitAmountText != null) unitAmountText.text = unitCount.ToString(); 
        }
    }

    void Update()
    {
        if (unitAmountText != null) unitAmountText.text = unitCount.ToString(); 
        if (fadedOverlay != null) 
        {
            fadedOverlay.SetActive(unitCount <= 0); 
        }
    }

    // --- HOVER EVENTS ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (unitCount <= 0) return; 

        if (hoverSFX != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayHoverSFX(hoverSFX);
        }

        AnimateScale(originalScale * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AnimateScale(originalScale);
    }

    // --- CLICK ANIMATION FEEDBACK ---
    public void OnPointerDown(PointerEventData eventData)
    {
        if (unitCount <= 0) return;

        // Play Click SFX
        if (clickSFX != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayClickSFX(clickSFX);
        }

        AnimateScale(originalScale * clickScale);
    }

    // --- GAMEPLAY ACTION ---
    public void ButtonClicked()
    {
        // Reset scale back to normal/hover
        AnimateScale(originalScale);

        Debug.Log(unitName); 

        BuildingSystem buildingSystem = Object.FindFirstObjectByType<BuildingSystem>(); 
        if (buildingSystem != null) 
        {
            if (buildingSystem.isBattleScene) 
            {
                buildingSystem.SelectBuildingByData(buildingData); 

                Object.FindFirstObjectByType<ToDoMessage>()?.ShowPrompt(); 
            }
            else
            {
                Debug.LogWarning("NOT IN BATTLE SCENE"); 
            }
        }
        else
        {
            Debug.LogError("NO BUILDING SYSTEM FOUND IN SCENE"); 
        }
    }
    private void AnimateScale(Vector3 targetScale)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleRoutine(targetScale));
    }

    private IEnumerator ScaleRoutine(Vector3 targetScale)
    {
        Vector3 initialScale = transform.localScale;
        float time = 0f;

        while (time < animDuration)
        {
            time += Time.unscaledDeltaTime; 
            transform.localScale = Vector3.Lerp(initialScale, targetScale, time / animDuration);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}