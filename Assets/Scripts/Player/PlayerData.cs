using UnityEngine;
using UnityEngine.SceneManagement;

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

    void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    void Awake()
    {
        updateUnitList();
        prepareBattleRoster();
    }

    void Start()
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

    public void updateUnitList()
    {
        if (playerBattleSO == null || playerBattleSO.playerUnitStats == null || playerBattleSO.playerUnits == null)
        {
            Debug.LogWarning("PlayerData Awake skipped: PlayerBattleSO or unit collections are not assigned.");
            return;
        }

        while (playerBattleSO.playerUnits.Count < playerBattleSO.playerUnitStats.Count)
        {
            playerBattleSO.playerUnits.Add(null);
        }

        for (int i = 0; i < playerBattleSO.playerUnitStats.Count; i++)
        {
            // Ensure that the playerUnitsBuilding list has the same number of elements as playerUnitStats
            if (playerBattleSO.playerUnitStats[i] != null && playerBattleSO.playerUnitStats[i].buildingData != null)
            {
                playerBattleSO.playerUnits[i] = playerBattleSO.playerUnitStats[i].buildingData;
            }
        }
    }

    public void prepareBattleRoster()
    {
        if (playerBattleSO == null)
        {
            return;
        }

        playerBattleSO.EnsureRuntimeLists();

        for (int i = 0; i < playerBattleSO.playerUnitStats.Count; i++)
        {
            UnitSO unit = playerBattleSO.playerUnitStats[i];
            if (unit != null)
            {
                playerBattleSO.playerUnitStats[i] = unit.CreateRuntimeCopy();
                if (playerBattleSO.playerUnitPrefabs[i] == null)
                {
                    playerBattleSO.playerUnitPrefabs[i] = playerBattleSO.playerUnitStats[i].unitPrefab;
                }
            }
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "BattleScene" || playerBattleSO == null)
        {
            return;
        }

        ApplyRuntimeUnitStatsToSceneUnits();
    }

    public void ApplyRuntimeUnitStatsToSceneUnits()
    {
        if (playerBattleSO == null || playerBattleSO.playerUnitStats == null)
        {
            return;
        }

        UnitSOContainer[] containers = Object.FindObjectsByType<UnitSOContainer>(FindObjectsSortMode.None);
        int rosterIndex = 0;

        for (int i = 0; i < containers.Length && rosterIndex < playerBattleSO.playerUnitStats.Count; i++)
        {
            UnitSOContainer container = containers[i];
            if (container == null)
            {
                continue;
            }

            if (!container.CompareTag("PlayerUnit") && container.GetComponentInParent<MoveUnit>() == null)
            {
                continue;
            }

            UnitSO runtimeUnit = playerBattleSO.playerUnitStats[rosterIndex];
            container.SetUnitData(runtimeUnit);

            MoveUnit moveUnit = container.GetComponent<MoveUnit>() ?? container.GetComponentInParent<MoveUnit>();
            if (moveUnit != null)
            {
                moveUnit.SyncFromContainer();
            }

            UnitHealth unitHealth = container.GetComponent<UnitHealth>() ?? container.GetComponentInParent<UnitHealth>();
            if (unitHealth != null)
            {
                unitHealth.SyncFromContainer();
            }

            HarvestResource harvestResource = container.GetComponent<HarvestResource>() ?? container.GetComponentInParent<HarvestResource>();
            if (harvestResource != null)
            {
                harvestResource.SyncFromContainer();
            }

            HarvestUnit harvestUnit = container.GetComponent<HarvestUnit>() ?? container.GetComponentInParent<HarvestUnit>();
            if (harvestUnit != null)
            {
                harvestUnit.SyncFromContainer();
            }

            AttackEnemyUnit attackEnemyUnit = container.GetComponent<AttackEnemyUnit>() ?? container.GetComponentInParent<AttackEnemyUnit>();
            if (attackEnemyUnit != null)
            {
                attackEnemyUnit.SyncFromContainer();
            }

            rosterIndex++;
        }
    }
}
