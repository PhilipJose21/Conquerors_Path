using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu()]
public class LevelSO : ScriptableObject
{
    public WorldSO levelName;
    public Sprite levelImage;
    public string levelSceneName;
    public int level;
    public bool isUnlocked;
    public bool isCompleted;

    private void OnEnable()
    {
        UpdateUnlockState();
    }

    private void OnValidate()
    {
        UpdateUnlockState();
        levelSceneName = name;
    }

    private void UpdateUnlockState()
    {
        if (level == 1)
        {
            isUnlocked = true;
        }
    }
}
