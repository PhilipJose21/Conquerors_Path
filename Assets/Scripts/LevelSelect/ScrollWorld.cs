using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class ScrollWorld : MonoBehaviour
{
    
    public GameObject worldSelectPanel;
    public GameObject levelInfoPanel;
    public List<GameObject> levelSelectPanel;
    public List<WorldSO> imagesPrefab;
    public List<Image> buttonImageObjects;
    public TextMeshProUGUI worldNameText;
    public int currentIndex = 0;

    void Awake()
    {
        for (int i = 0; i < levelSelectPanel.Count; i++)
        {
            levelSelectPanel[i].SetActive(false);
        }
        worldSelectPanel.SetActive(true);
    }

    void Start()
    {
        UpdateImages();
    }

    public void openLevelSelect()
    {
        worldSelectPanel.SetActive(false);
        levelSelectPanel[currentIndex].SetActive(true);
    }

    public void loadLevel()
    {
        SceneManager.LoadScene(imagesPrefab[currentIndex].worldLevelScene);
    }

    public void ScrollLeft()
    {
        currentIndex--;
        UpdateImages();
    }

    public void ScrollRight()
    {
        currentIndex++;
        UpdateImages();
    }

    public void UpdateImages()
    {
        if (imagesPrefab == null || buttonImageObjects == null || imagesPrefab.Count == 0 || buttonImageObjects.Count < 3)
        {
            return;
        }

        currentIndex = ((currentIndex % imagesPrefab.Count) + imagesPrefab.Count) % imagesPrefab.Count;

        buttonImageObjects[0].sprite = imagesPrefab[(currentIndex - 1 + imagesPrefab.Count) % imagesPrefab.Count].worldImage;
        buttonImageObjects[1].sprite = imagesPrefab[currentIndex].worldImage;
        buttonImageObjects[2].sprite = imagesPrefab[(currentIndex + 1) % imagesPrefab.Count].worldImage;

        worldNameText.text = imagesPrefab[currentIndex].worldName;
    }

    public void BackButton()
    {
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
