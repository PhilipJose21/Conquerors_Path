using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScrollWorld : MonoBehaviour
{
    public GameObject worldSelectPanel;
    public GameObject levelInfoPanel;
    public List<GameObject> levelSelectPanel;
    public List<WorldSO> imagesPrefab;
    public List<Image> buttonImageObjects; // [0] = Left, [1] = Center, [2] = Right
    public TextMeshProUGUI worldNameText;
    public int currentIndex = 0;

    [Header("Main Scene Background Link")]
    [SerializeField] private Image sceneBackgroundImage;       // Drag your main "BG" Image object here

    [Header("Roll Animation Settings")]
    [SerializeField] private float transitionDuration = 0.25f; 
    
    [Header("Spherical Color Settings")]
    [SerializeField] private Color normalColor = Color.white;                    
    [SerializeField] private Color greyedOutColor = new Color(0.4f, 0.4f, 0.4f, 1f); 

    private Vector2[] basePositions;
    private Vector2[] baseScales;
    private bool isAnimating = false;

    void Awake()
    {
        for (int i = 0; i < levelSelectPanel.Count; i++)
        {
            levelSelectPanel[i].SetActive(false);
        }
        worldSelectPanel.SetActive(true);
        if (levelInfoPanel != null)
        {
            levelInfoPanel.SetActive(false);
        }

        RecordOriginalLayoutPositions();
    }

    void Start()
    {
        UpdateImagesInstant();
    }

    private void RecordOriginalLayoutPositions()
    {
        if (buttonImageObjects == null || buttonImageObjects.Count < 3) return;

        basePositions = new Vector2[3];
        baseScales = new Vector2[3];

        for (int i = 0; i < 3; i++)
        {
            basePositions[i] = buttonImageObjects[i].rectTransform.anchoredPosition;
            baseScales[i] = buttonImageObjects[i].rectTransform.localScale;
        }
    }

    public void ScrollLeft()
    {
        if (isAnimating || imagesPrefab.Count <= 1) return;
        StartCoroutine(AnimateRoll(true));
    }

    public void ScrollRight()
    {
        if (isAnimating || imagesPrefab.Count <= 1) return;
        StartCoroutine(AnimateRoll(false));
    }

    private Color GetHardcodedColor(string worldName)
    {
        if (string.IsNullOrEmpty(worldName)) return Color.white;

        string nameCheck = worldName.Trim().ToLower();

        if (nameCheck.Contains("plain"))
        {
            ColorUtility.TryParseHtmlString("#52D622", out Color plainsGreen);
            return plainsGreen;
        }
        else if (nameCheck.Contains("desert"))
        {
            ColorUtility.TryParseHtmlString("#FFC44D", out Color desertYellow);
            return desertYellow;
        }
        else if (nameCheck.Contains("mountain"))
        {
            ColorUtility.TryParseHtmlString("#D1DADE", out Color mountainsGray);
            return mountainsGray;
        }
        return Color.white; 
    }

    private IEnumerator AnimateRoll(bool scrollingLeft)
    {
        isAnimating = true;

        // Cache the current background color tint as our starting baseline point
        Color startBGColor = sceneBackgroundImage != null ? sceneBackgroundImage.color : Color.white;

        if (scrollingLeft) currentIndex--;
        else currentIndex++;
        currentIndex = ((currentIndex % imagesPrefab.Count) + imagesPrefab.Count) % imagesPrefab.Count;

        // Identify the target hardcoded color based on the name text string
        Color targetBGColor = imagesPrefab[currentIndex] != null ? GetHardcodedColor(imagesPrefab[currentIndex].worldName) : Color.white;

        float elapsed = 0f;

        Sprite incomingSpriteLeft = imagesPrefab[(currentIndex - 1 + imagesPrefab.Count) % imagesPrefab.Count].worldImage;
        Sprite incomingSpriteRight = imagesPrefab[(currentIndex + 1) % imagesPrefab.Count].worldImage;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            // DYNAMIC BACKGROUND TINT LERP: Smoothly blends the color values frame-by-frame
            if (sceneBackgroundImage != null)
            {
                sceneBackgroundImage.color = Color.Lerp(startBGColor, targetBGColor, t);
            }

            if (scrollingLeft)
            {
                buttonImageObjects[0].rectTransform.anchoredPosition = Vector2.Lerp(basePositions[0], basePositions[1], t);
                buttonImageObjects[0].rectTransform.localScale = Vector2.Lerp(baseScales[0], baseScales[1], t);
                buttonImageObjects[0].color = Color.Lerp(greyedOutColor, normalColor, t);
                
                buttonImageObjects[1].rectTransform.anchoredPosition = Vector2.Lerp(basePositions[1], basePositions[2], t);
                buttonImageObjects[1].rectTransform.localScale = Vector2.Lerp(baseScales[1], baseScales[2], t);
                buttonImageObjects[1].color = Color.Lerp(normalColor, greyedOutColor, t);
                
                buttonImageObjects[2].rectTransform.anchoredPosition = Vector2.Lerp(basePositions[2], basePositions[0], t);
                buttonImageObjects[2].rectTransform.localScale = Vector2.Lerp(baseScales[2], baseScales[0] * 0.75f, t);
                buttonImageObjects[2].color = greyedOutColor;
                buttonImageObjects[2].sprite = incomingSpriteLeft;
            }
            else
            {
                buttonImageObjects[0].rectTransform.anchoredPosition = Vector2.Lerp(basePositions[0], basePositions[2], t);
                buttonImageObjects[0].rectTransform.localScale = Vector2.Lerp(baseScales[0], baseScales[2] * 0.75f, t);
                buttonImageObjects[0].color = greyedOutColor;
                buttonImageObjects[0].sprite = incomingSpriteRight;

                buttonImageObjects[1].rectTransform.anchoredPosition = Vector2.Lerp(basePositions[1], basePositions[0], t);
                buttonImageObjects[1].rectTransform.localScale = Vector2.Lerp(baseScales[1], baseScales[0], t);
                buttonImageObjects[1].color = Color.Lerp(normalColor, greyedOutColor, t);
                
                buttonImageObjects[2].rectTransform.anchoredPosition = Vector2.Lerp(basePositions[2], basePositions[1], t);
                buttonImageObjects[2].rectTransform.localScale = Vector2.Lerp(baseScales[2], baseScales[1], t);
                buttonImageObjects[2].color = Color.Lerp(greyedOutColor, normalColor, t);
            }

            yield return null;
        }

        UpdateImagesInstant();
        isAnimating = false;
    }

    public void UpdateImagesInstant()
    {
        if (imagesPrefab == null || buttonImageObjects == null || imagesPrefab.Count == 0 || buttonImageObjects.Count < 3) return;

        currentIndex = ((currentIndex % imagesPrefab.Count) + imagesPrefab.Count) % imagesPrefab.Count;

        for (int i = 0; i < 3; i++)
        {
            buttonImageObjects[i].rectTransform.anchoredPosition = basePositions[i];
            buttonImageObjects[i].rectTransform.localScale = baseScales[i];
            buttonImageObjects[i].color = (i == 1) ? normalColor : greyedOutColor;
        }

        // Ensure the scene background color matches on instant loading snaps or script boots
        if (sceneBackgroundImage != null && imagesPrefab[currentIndex] != null)
        {
            sceneBackgroundImage.color = GetHardcodedColor(imagesPrefab[currentIndex].worldName);
        }

        buttonImageObjects[0].sprite = imagesPrefab[(currentIndex - 1 + imagesPrefab.Count) % imagesPrefab.Count].worldImage;
        buttonImageObjects[1].sprite = imagesPrefab[currentIndex].worldImage;
        buttonImageObjects[2].sprite = imagesPrefab[(currentIndex + 1) % imagesPrefab.Count].worldImage;

        worldNameText.text = imagesPrefab[currentIndex].worldName;
    }

    public void openLevelSelect()
    {
        worldSelectPanel.SetActive(false);
        levelSelectPanel[currentIndex].SetActive(true);
    }

    public void loadLevel()
    {
        KingdomSaveManager.Instance?.SaveCurrentKingdom();
        SceneManager.LoadScene(imagesPrefab[currentIndex].worldLevelScene);
    }

    public void BackButton()
    {
        KingdomSaveManager.Instance?.SaveCurrentKingdom();

        if (worldSelectPanel.activeSelf)
        {
            SceneManager.LoadScene("MainKingdom");
        }
        else
        {
            worldSelectPanel.SetActive(true);
            levelSelectPanel[currentIndex].SetActive(false);
            levelInfoPanel.SetActive(false);
        }
    }
}