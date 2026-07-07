using UnityEngine;
using System.Collections.Generic;

public class LevelsUnlocked : MonoBehaviour
{
    [Header("Player Data")]
    public PlayerBattleSO playerBattleSO;

    [Header("Button Materials")]
    public Material lockedMaterial;
    public Material unlockedMaterial;
    public Material completedMaterial;

    [Header("Level Data")]
    public List<WorldSO> worlds;
    private List<LevelSO> levels;
    public List<LevelSO> levelsUnlocked;
    public List<LevelSO> levelsCompleted;

    void Awake()
    {
        playerBattleSO = FindObjectOfType<PlayerData>().playerBattleSO;
        levelsUnlocked.Clear();
        levelsCompleted.Clear();

        for (int i = 0; i < worlds.Count; i++)
        {
            levels = worlds[i].levels;
            for (int j = 0; j < levels.Count; j++)
            {
                if (levels[j].isUnlocked && !levelsUnlocked.Contains(levels[j]))
                {
                    levelsUnlocked.Add(levels[j]);
                }
                if (levels[j].isCompleted && !levelsCompleted.Contains(levels[j]))
                {
                    levelsCompleted.Add(levels[j]);
                }
            }
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < worlds.Count; i++)
        {
            levels = worlds[i].levels;
            for (int j = 0; j < levels.Count; j++)
            {
                if (levels[j].isCompleted)
                {
                    if (j + 1 >= levels.Count)
                    {
                        continue;
                    }

                    levels[j + 1].isUnlocked = true;
                    if (levels[j + 1].isUnlocked && !levelsUnlocked.Contains(levels[j + 1]))
                    {
                        levelsUnlocked.Add(levels[j + 1]);
                    }
                }
            }
        }
    }
}
