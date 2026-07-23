using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MinimizedInspector : MonoBehaviour
{
    public static MinimizedInspector Instance { get; private set; }

    [Header("Core UI Fields (Always Visible)")]
    public Image unitIconImage; 
    public GameObject toggleButton; 

    [Header("Detailed Fields (Hidden when Minimized)")]
    public TextMeshProUGUI lvlText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI healthText;
    public Image healthBarFill;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI rangeText;

    [Header("Dead Unit Visual Settings")]
    [SerializeField] private Color deadUnitIconColor = new Color(0.3f, 0.3f, 0.3f, 0.8f); 

    [HideInInspector] public bool isLockedOpen = false;
    private bool isExpanded = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        gameObject.SetActive(false); 
        isExpanded = false;
        isLockedOpen = false;
    }

    public void ShowUnitStats(UnitSO unitData, int currentHP, int maxHP)
    {
        if (unitData == null) return;
        
        bool wasActive = gameObject.activeSelf;

        gameObject.SetActive(true);
        isLockedOpen = true;

        if (unitIconImage != null)
        {
            if (unitData.unitIcon != null)
            {
                unitIconImage.sprite = unitData.unitIcon;
                unitIconImage.color = (currentHP <= 0) ? deadUnitIconColor : Color.white; 
            }
            else
            {
                Debug.LogWarning($"[UI Warning] UnitSO '{unitData.unitName}' has NO icon assigned!");
            }
        }

        if (nameText != null) nameText.text = unitData.unitName;
        if (healthText != null) healthText.text = $"Health: {Mathf.Max(0, currentHP)} / {maxHP}";
        if (lvlText != null) lvlText.text = $"{unitData.level}"; 
        if (attackText != null) attackText.text = $"Atk: {unitData.damage}";
        if (rangeText != null) rangeText.text = $"Range: {unitData.attackRange}x{unitData.attackRange}";

        if (healthBarFill != null) 
        {
            float fill = maxHP > 0 ? (float)Mathf.Max(0, currentHP) / maxHP : 0f;
            healthBarFill.fillAmount = Mathf.Clamp01(fill);
        }

        if (!wasActive)
        {
            SetExpandedState(false); 
            PlayPopAnimation();
        }
        else
        {
            SetExpandedState(isExpanded); 
        }
    }

    public void ToggleExpand()
    {
        SetExpandedState(!isExpanded);
    }

    public void SetExpandedState(bool expand)
    {
        isExpanded = expand;

        // Toggle stats sub-elements cleanly
        if (nameText != null) nameText.gameObject.SetActive(isExpanded);
        if (healthText != null) healthText.gameObject.SetActive(isExpanded);
        if (attackText != null) attackText.gameObject.SetActive(isExpanded);
        if (rangeText != null) rangeText.gameObject.SetActive(isExpanded);

        if (healthBarFill != null && healthBarFill.transform.parent != null)
        {
            healthBarFill.transform.parent.gameObject.SetActive(isExpanded);
        }
    }

    public void CloseInspector()
    {
        isLockedOpen = false;
        isExpanded = false;
        gameObject.SetActive(false);
    }

    public void PlayPopAnimation()
    {
        StopAllCoroutines();
        StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        RectTransform rect = GetComponent<RectTransform>();
        
        rect.localScale = new Vector3(0.7f, 0.7f, 1f);
        float time = 0f;
        float duration = 0.2f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float percent = time / duration;
            
            float scaleVal = Mathf.Lerp(0.7f, 1f, Mathf.Sin(percent * Mathf.PI * 0.5f));
            rect.localScale = new Vector3(scaleVal, scaleVal, 1f);
            yield return null;
        }
        
        rect.localScale = Vector3.one;
    }
}