using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadLevel : MonoBehaviour
{
    public string sceneName;
    public void LoadLevelScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
