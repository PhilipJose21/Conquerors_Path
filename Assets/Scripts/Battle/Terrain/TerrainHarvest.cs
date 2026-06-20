using UnityEngine;

public class TerrainHarvest : MonoBehaviour
{
    public TerrainSOContainer terrainDataContainer;
    private TerrainSO terrainData;
    
    [SerializeField] private PlayerBattleSO playerBattleData; 
    public TerrainSO.ResourceType resourceType;
    public TerrainSO.ResourceType secondaryResourceType;
    public bool hasHarvested = false;

    void Awake()
    {
        terrainDataContainer = GetComponent<TerrainSOContainer>();
        if (terrainDataContainer != null && terrainDataContainer.terrainData != null)
        {
            terrainData = terrainDataContainer.terrainData;
            resourceType = terrainData.resourceType;
        }
    }

    public void Harvest(int amount, MoveUnit unitMovement)
    {
        if (hasHarvested) return;

        // 2. Consume an Attack Point
        if (unitMovement != null)
        {
            unitMovement.attackActions = Mathf.Max(0, unitMovement.attackActions - 1);
        }

        // 3 & 4. Gain assigned resource type and add to PlayerBattleSO based on unit's harvestAmount
        if (playerBattleData != null)
        {
            int harvestAmount = amount;
            if (secondaryResourceType != TerrainSO.ResourceType.None)
            {
                harvestAmount = Mathf.CeilToInt(amount / 2f);
                switch (secondaryResourceType)
                {
                    case TerrainSO.ResourceType.Wood:
                        playerBattleData.woodHarvestAmount += harvestAmount;
                        break;
                    case TerrainSO.ResourceType.Stone:
                        playerBattleData.stoneHarvestAmount += harvestAmount;
                        break;
                    case TerrainSO.ResourceType.Farm:
                        playerBattleData.farmHarvestAmount += harvestAmount;
                        break;
                    case TerrainSO.ResourceType.Coins:
                        playerBattleData.goldHarvestAmount += harvestAmount;
                        break;
                    default:
                        Debug.Log($"Harvested {amount} of {secondaryResourceType}, but it doesn't have an explicitly mapped tracker integer in PlayerBattleSO yet.");
                        break;
                }
            }
            switch (resourceType)
            {
                case TerrainSO.ResourceType.Wood:
                    playerBattleData.woodHarvestAmount += harvestAmount;
                    break;
                case TerrainSO.ResourceType.Stone:
                    playerBattleData.stoneHarvestAmount += harvestAmount;
                    break;
                case TerrainSO.ResourceType.Farm:
                    playerBattleData.farmHarvestAmount += harvestAmount;
                    break;
                case TerrainSO.ResourceType.Coins:
                    playerBattleData.goldHarvestAmount += harvestAmount;
                    break;
                default:
                    Debug.Log($"Harvested {amount} of {resourceType}, but it doesn't have an explicitly mapped tracker integer in PlayerBattleSO yet.");
                    break;
            }


            Debug.Log($"Successfully added {harvestAmount} to your player battle storage for {resourceType}!");
        }
        else
        {
            Debug.LogWarning("PlayerBattleSO references are missing on this TerrainHarvest node configuration!");
        }

        // 5. Make hasHarvested true to prevent the player from harvesting the same terrain node again
        hasHarvested = true;
    }
}