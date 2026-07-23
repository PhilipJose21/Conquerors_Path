using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToMainmenu : MonoBehaviour
{
    void Awake()
    {
        this.gameObject.SetActive(false);
    }
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void ClosePanel()
    {
        this.gameObject.SetActive(false);
    }
}
