using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadLevel : MonoBehaviour
{
    public bool loadCurrentLevel;
    public string sceneName;

    void Awake()
    {
        if (loadCurrentLevel)
        {
            PlayerBattleSO playerBattleSO = FindObjectOfType<PlayerData>().playerBattleSO;
            sceneName = playerBattleSO.currentLevel.levelSceneName;
        }
    }

    public void LoadLevelScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
