using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class ScrollWorld : MonoBehaviour
{
    public List<LevelSO> imagesPrefab;
    public List<Image> buttonImageObjects;
    public TextMeshProUGUI worldNameText;
    public int currentIndex = 0;

    void Start()
    {
        UpdateImages();
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
}
