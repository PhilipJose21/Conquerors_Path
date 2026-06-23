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
    public bool isBattleScene = false;

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

        if (playerBattleSO == null)
        {
            Debug.Log("No PlayerBattleSO");
        }
        playerWoodResources = playerSO.woodResources;
        playerStoneResources = playerSO.stoneResources;
        playerFarmResources = playerSO.farmResources;
        playerEnergyPoints = playerSO.energyPoints;
        playerResearchPoints = playerSO.researchPoints;
        playerGems = playerSO.gems;
        playerCoins = playerSO.coins;
        if (playerBattleSO != null && isBattleScene)
        {
            updatePlayerMaterials();
        }
    }

    void Update()
    {
        updatePlayerMaterials();
    }

    public void updatePlayerMaterials()
    {
        playerWoodResources = playerSO.woodResources;
        playerStoneResources = playerSO.stoneResources;
        playerFarmResources = playerSO.farmResources;
        playerEnergyPoints = playerSO.energyPoints;
        playerResearchPoints = playerSO.researchPoints;
        playerGems = playerSO.gems;
        playerCoins = playerSO.coins;
    }
}
