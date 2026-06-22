using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    [Header("Core Scriptable Object References")]
    public PlayerSO playerSO;
    public PlayerBattleSO playerBattleSO;

    public int playerWood;
    public int playerStone;
    public int playerFarm;
    public int playerGold;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        playerWood = playerBattleSO.woodHarvestAmount;
        playerStone = playerBattleSO.stoneHarvestAmount;
        playerFarm = playerBattleSO.farmHarvestAmount;
        playerGold = playerBattleSO.goldHarvestAmount;
    }
}
