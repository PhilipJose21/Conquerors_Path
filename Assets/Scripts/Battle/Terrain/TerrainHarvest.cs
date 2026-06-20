using UnityEngine;

public class TerrainHarvest : MonoBehaviour
{
    private TerrainSOContainer terrainSOContainer;
    private TerrainSO terrainSO;
    public TerrainSO.ResourceType resourceType;
    public TerrainSO.ResourceType secondaryResourceType;
    
    public PlayerData playerbattleData;
    public PlayerBattleSO playerBattleSO;
    public bool canHarvest = true;

    void Awake()
    {
        playerbattleData = FindObjectOfType<PlayerData>();
        terrainSOContainer = this.GetComponent<TerrainSOContainer>();

        terrainSO = terrainSOContainer.terrainData;
        resourceType = terrainSO.resourceType;
        secondaryResourceType = terrainSO.secondaryResourceType;

        playerBattleSO = playerbattleData.playerBattleSO;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HarvestResource(int amount)
    {
        if (canHarvest)
        {
            int harvestAmount = amount;
            if (terrainSO.secondaryResourceType != TerrainSO.ResourceType.None)
            {
                harvestAmount /= 2; 
            }
            addMainResource(harvestAmount);
            addSecondaryResource(harvestAmount);
            canHarvest = false;
        }
    }

    public void addMainResource(int amount)
    {
        switch (resourceType)
        {
            case TerrainSO.ResourceType.Wood:
                playerBattleSO.woodHarvestAmount += amount;
                break;
            case TerrainSO.ResourceType.Stone:
                playerBattleSO.stoneHarvestAmount += amount;
                break;
            case TerrainSO.ResourceType.Farm:
                playerBattleSO.farmHarvestAmount += amount;
                break;
            case TerrainSO.ResourceType.Coins:
                playerBattleSO.goldHarvestAmount += amount;
                break; 
            default:
                Debug.LogWarning("Unknown resource type: " + resourceType);
                break;
        }
        Debug.Log("Harvested " + amount + " of " + resourceType);
    }

    public void addSecondaryResource(int amount)
    {
        switch (secondaryResourceType)
        {
            case TerrainSO.ResourceType.Wood:
                playerBattleSO.woodHarvestAmount += amount;
                break;
            case TerrainSO.ResourceType.Stone:
                playerBattleSO.stoneHarvestAmount += amount;
                break;
            case TerrainSO.ResourceType.Farm:
                playerBattleSO.farmHarvestAmount += amount;
                break;
            case TerrainSO.ResourceType.Coins:
                playerBattleSO.goldHarvestAmount += amount;
                break; 
            default:
                Debug.LogWarning("Unknown resource type: " + secondaryResourceType);
                break;
        }
        Debug.Log("Harvested " + amount + " of " + secondaryResourceType);
    }
}
