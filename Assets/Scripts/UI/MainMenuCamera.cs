using UnityEngine;
using Unity.Cinemachine;
using TMPro;

public class MainMenuCamera : MonoBehaviour
{
    public CinemachineCamera mainMenuCamera;
    public CinemachineCamera titleScreenCamera;
    public GameObject mainMenuUI;
    public TextMeshProUGUI titleText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        titleScreenCamera.Priority = 2;
        mainMenuCamera.Priority = 1;
        titleText.gameObject.SetActive(true);
        mainMenuUI.SetActive(false);
    }

    void Update()
    {
        if (Input.anyKeyDown && titleScreenCamera.Priority == 2)
        {
            titleText.gameObject.SetActive(false);
            mainMenuUI.SetActive(true);
            titleScreenCamera.Priority = 1;
            mainMenuCamera.Priority = 2;
        }
    }
}
