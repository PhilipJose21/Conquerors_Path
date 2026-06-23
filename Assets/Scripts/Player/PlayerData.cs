using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    [Header("Core Scriptable Object References")]
    public PlayerSO playerSO;
    public PlayerBattleSO playerBattleSO;


    public int playerWoodResources;
    public int playerStoneResources;
    public int playerFarmResources;
    public int playerEnergyPoints;
    public int playerResearchPoints;
    public int playerGems;
    public int playerCoins;

    public bool isBattleScene;
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
        // Initialization moved to Start() so other objects/SOs have a chance to be assigned.
    }

    void Start()
    {
        // Prefer initializing from the runtime Battle SO if available (it may change during gameplay).
        if (playerBattleSO != null && isBattleScene)
        {
            playerWoodResources = playerBattleSO.woodHarvestAmount;
            playerStoneResources = playerBattleSO.stoneHarvestAmount;
            playerFarmResources = playerBattleSO.farmHarvestAmount;
            playerCoins = playerBattleSO.goldHarvestAmount;
            Debug.Log($"PlayerData initialized from PlayerBattleSO: wood={playerWoodResources} stone={playerStoneResources} farm={playerFarmResources} coins={playerCoins}");
        }
        else if (playerSO != null)
        {
            playerWoodResources = playerSO.woodResources;
            playerStoneResources = playerSO.stoneResources;
            playerFarmResources = playerSO.farmResources;
            playerEnergyPoints = playerSO.energyPoints;
            playerResearchPoints = playerSO.researchPoints;
            playerGems = playerSO.gems;
            playerCoins = playerSO.coins;
            Debug.Log($"PlayerData initialized from PlayerSO: wood={playerWoodResources} stone={playerStoneResources} farm={playerFarmResources} coins={playerCoins}");
        }
        else
        {
            Debug.LogWarning("PlayerData: No PlayerSO or PlayerBattleSO assigned in inspector.");
        }
    }

    void Update()
    {
        if (playerBattleSO != null)
        {
            updatePlayerMaterials();
        }
    }

    public void updatePlayerMaterials()
    {
        if (playerBattleSO == null)
        {
            Debug.LogWarning("updatePlayerMaterials called but playerBattleSO is null");
            return;
        }

        int oldWood = playerWoodResources;
        int oldStone = playerStoneResources;
        int oldFarm = playerFarmResources;
        int oldCoins = playerCoins;

        playerWoodResources = playerBattleSO.woodHarvestAmount;
        playerStoneResources = playerBattleSO.stoneHarvestAmount;
        playerFarmResources = playerBattleSO.farmHarvestAmount;
        playerCoins = playerBattleSO.goldHarvestAmount;

        if (playerWoodResources != oldWood || playerStoneResources != oldStone || playerFarmResources != oldFarm || playerCoins != oldCoins)
        {
            Debug.Log($"PlayerData updated from PlayerBattleSO: wood {oldWood}->{playerWoodResources}, stone {oldStone}->{playerStoneResources}, farm {oldFarm}->{playerFarmResources}, coins {oldCoins}->{playerCoins}");
        }
        Debug.Log("Test");
    }
}
