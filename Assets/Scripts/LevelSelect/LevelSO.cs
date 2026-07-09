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
    public int levelReinforcementCost;

    [Header("Reward")]
    public bool rewardClaimed;
    public int energyPointsReward;
    public int gemsReward;
    public int coinsReward;

    private void OnEnable()
    {
    }

    private void OnValidate()
    {
        levelSceneName = name;
    }
}
