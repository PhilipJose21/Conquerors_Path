using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

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
    [SerializeField] private Image sceneBackgroundImage;

    [Header("Roll Animation Settings")]
    [SerializeField] private float transitionDuration = 0.25f; 
    
    [Header("Spherical Color Settings")]
    [SerializeField] private Color normalColor = Color.white;                    
    [SerializeField] private Color greyedOutColor = new Color(0.4f, 0.4f, 0.4f, 1f); 

    private Vector2[] basePositions;
    private Vector3 centerScale = Vector3.one;
    private Vector3 sideScale = new Vector3(0.7f, 0.7f, 1f); // 🌟 Define side scale directly
    private bool isAnimating = false;

    void Awake()
    {
        for (int i = 0; i < levelSelectPanel.Count; i++)
        {
            if (levelSelectPanel[i] != null) levelSelectPanel[i].SetActive(false);
        }
        if (worldSelectPanel != null) worldSelectPanel.SetActive(true);
        if (levelInfoPanel != null) levelInfoPanel.SetActive(false);

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

        for (int i = 0; i < 3; i++)
        {
            basePositions[i] = buttonImageObjects[i].rectTransform.anchoredPosition;
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

        Color startBGColor = sceneBackgroundImage != null ? sceneBackgroundImage.color : Color.white;

        if (scrollingLeft) currentIndex--;
        else currentIndex++;
        currentIndex = ((currentIndex % imagesPrefab.Count) + imagesPrefab.Count) % imagesPrefab.Count;

        Color targetBGColor = imagesPrefab[currentIndex] != null ? GetHardcodedColor(imagesPrefab[currentIndex].worldName) : Color.white;

        float elapsed = 0f;

        Sprite incomingSpriteLeft = imagesPrefab[(currentIndex - 1 + imagesPrefab.Count) % imagesPrefab.Count].worldImage;
        Sprite incomingSpriteRight = imagesPrefab[(currentIndex + 1) % imagesPrefab.Count].worldImage;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            t = Mathf.SmoothStep(0f, 1f, t); 

            if (sceneBackgroundImage != null)
            {
                sceneBackgroundImage.color = Color.Lerp(startBGColor, targetBGColor, t);
            }

            if (scrollingLeft)
            {
                // Left [0] -> Center [1] (Scales UP from sideScale to centerScale)
                buttonImageObjects[0].rectTransform.anchoredPosition = Vector2.Lerp(basePositions[0], basePositions[1], t);
                buttonImageObjects[0].rectTransform.localScale = Vector3.Lerp(sideScale, centerScale, t);
                buttonImageObjects[0].color = Color.Lerp(greyedOutColor, normalColor, t);
                
                // Center [1] -> Right [2] (Scales DOWN from centerScale to sideScale)
                buttonImageObjects[1].rectTransform.anchoredPosition = Vector2.Lerp(basePositions[1], basePositions[2], t);
                buttonImageObjects[1].rectTransform.localScale = Vector3.Lerp(centerScale, sideScale, t);
                buttonImageObjects[1].color = Color.Lerp(normalColor, greyedOutColor, t);
                
                // Right [2] -> Loops to Left [0] (Stays sideScale)
                buttonImageObjects[2].rectTransform.anchoredPosition = Vector2.Lerp(basePositions[2], basePositions[0], t);
                buttonImageObjects[2].rectTransform.localScale = sideScale; 
                buttonImageObjects[2].color = greyedOutColor;
                buttonImageObjects[2].sprite = incomingSpriteLeft;
            }
            else
            {
                // Left [0] -> Loops to Right [2] (Stays sideScale)
                buttonImageObjects[0].rectTransform.anchoredPosition = Vector2.Lerp(basePositions[0], basePositions[2], t);
                buttonImageObjects[0].rectTransform.localScale = sideScale; 
                buttonImageObjects[0].color = greyedOutColor;
                buttonImageObjects[0].sprite = incomingSpriteRight;

                // Center [1] -> Left [0] (Scales DOWN from centerScale to sideScale)
                buttonImageObjects[1].rectTransform.anchoredPosition = Vector2.Lerp(basePositions[1], basePositions[0], t);
                buttonImageObjects[1].rectTransform.localScale = Vector3.Lerp(centerScale, sideScale, t);
                buttonImageObjects[1].color = Color.Lerp(normalColor, greyedOutColor, t);
                
                // Right [2] -> Center [1] (Scales UP from sideScale to centerScale)
                buttonImageObjects[2].rectTransform.anchoredPosition = Vector2.Lerp(basePositions[2], basePositions[1], t);
                buttonImageObjects[2].rectTransform.localScale = Vector3.Lerp(sideScale, centerScale, t);
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
            
            // 🌟 Set resting scale explicitly (1.0 for Center, 0.7 for Sides)
            buttonImageObjects[i].rectTransform.localScale = (i == 1) ? centerScale : sideScale;
            buttonImageObjects[i].color = (i == 1) ? normalColor : greyedOutColor;
            buttonImageObjects[i].raycastTarget = (i == 1);
        }

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
            if (levelInfoPanel != null) levelInfoPanel.SetActive(false);
        }
    }
}