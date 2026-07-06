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
    public List<string> levelsUnlocked;
    public List<string> levelsCompleted;

    void Awake()
    {
        playerBattleSO = FindObjectOfType<PlayerData>().playerBattleSO;

    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
