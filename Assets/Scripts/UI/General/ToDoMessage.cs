using UnityEngine;
using TMPro;

public class ToDoMessage : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Prompt Settings")]
    [SerializeField] private string cancelMessage = "Press Q to Cancel";

    private void Start()
    {
        if (promptText != null)
        {
            promptText.text = cancelMessage;
            promptText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // Automatically hide the prompt if the player presses 'Q'
        if (Input.GetKeyDown(KeyCode.Q))
        {
            HidePrompt();
        }
    }
    public void ShowPrompt()
    {
        if (promptText != null)
        {
            promptText.text = cancelMessage;
            promptText.gameObject.SetActive(true);
        }
    }

    public void HidePrompt()
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }
}