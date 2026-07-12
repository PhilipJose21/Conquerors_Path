using UnityEngine;

public class PassiveResource : MonoBehaviour
{
    // Component that periodically awards a resource to the player while active.
    // It increments a PlayerSO resource field every `resourceTimer` seconds.
    public BuildingStatsSO.ResourceType resourceType;
    public PlayerData playerData => UnityEngine.Object.FindFirstObjectByType<PlayerData>();
    public PlayerSO playerSO => playerData.playerSO;

    public BuildingStatsSO buildingStatsSO;
    public BuildingData buildingData; 
    public int level = 1;
    public bool isActive;
    public int resourceAmount;
    private int totalResourceAmount;
    public float resourceTimer;
    public float currentTime;

    public int coinCost;
    public int farmCost;
    public int rockCost;
    public int woodCost;
    public int gemCost;
    public int energyCost;

    private bool hasLoadedSavedState;

    void Awake()
    {
        
    }

    void Start()
    {
        BuildingStatContainer statContainer = GetComponent<BuildingStatContainer>();
        
        buildingStatsSO = statContainer?.buildingStatsSO;
        buildingData = statContainer?.buildingData;

        if (buildingStatsSO != null)
        {
            resourceType = buildingStatsSO.resourceType;
            resourceTimer = buildingStatsSO.resourceTimer;
        }

        if (hasLoadedSavedState)
        {
            return;
        }

        if (buildingStatsSO != null)
        {
            resourceAmount = buildingStatsSO.resourceAmount;
        }

        if (buildingData != null)
        {
            coinCost = buildingData.coinCost;
            farmCost = buildingData.farmCost;
            rockCost = buildingData.rockCost;
            woodCost = buildingData.woodCost;
            gemCost = buildingData.gemCost;
            energyCost = buildingData.energyCost;
            changeUpgradeCost();
        }
    }

    void Update()
    {
        // Only accrue while active
        if (!isActive)
            return;

        // A misconfigured/unset resourceTimer (0 or negative) would otherwise satisfy
        // "currentTime >= resourceTimer" on every single frame, firing AddResource
        // dozens of times a second instead of once per interval.
        if (resourceTimer <= 0f)
            return;

        // Track elapsed time and award resource when timer completes
        currentTime += Time.deltaTime;

        if (currentTime >= resourceTimer)
        {
            currentTime = 0f;
            AddResource(resourceType);
        }
        
    }

    public void AddResource(BuildingStatsSO.ResourceType type)
    {
        // Add the configured resource amount to the player's SO based on type.
        // resourceAmount can be negative (an upkeep/drain building), so every branch
        // is clamped to a floor of 0 - passive ticks must never push a resource
        // negative, regardless of sign or how large resourceAmount has grown from
        // repeated upgrades (see increaseStats()).
        switch (type)
        {
            case BuildingStatsSO.ResourceType.Wood:
                playerSO.woodResources = Mathf.Max(0, playerSO.woodResources + resourceAmount);
                break;
            case BuildingStatsSO.ResourceType.Stone:
                playerSO.stoneResources = Mathf.Max(0, playerSO.stoneResources + resourceAmount);
                break;
            case BuildingStatsSO.ResourceType.Farm:
                playerSO.farmResources = Mathf.Max(0, playerSO.farmResources + resourceAmount);
                break;
            case BuildingStatsSO.ResourceType.Energy:
                playerSO.energyPoints = Mathf.Max(0, playerSO.energyPoints + resourceAmount);
                break;
            case BuildingStatsSO.ResourceType.Research:
                playerSO.researchPoints = Mathf.Max(0, playerSO.researchPoints + resourceAmount);
                break;
            case BuildingStatsSO.ResourceType.Gems:
                playerSO.gems = Mathf.Max(0, playerSO.gems + resourceAmount);
                break;
            case BuildingStatsSO.ResourceType.Coins:
                playerSO.coins = Mathf.Max(0, playerSO.coins + resourceAmount);
                break;
        }

        // Track cumulative amount awarded (useful for stats/debugging)
        totalResourceAmount += resourceAmount;
    }

    public void upgradeBuilding()
    {
        Debug.Log("Attempting to upgrade building...");
        if (playerSO.woodResources >= woodCost && playerSO.stoneResources >= rockCost && playerSO.farmResources >= farmCost && playerSO.coins >= coinCost)
        {
            playerSO.woodResources = Mathf.Max(0, playerSO.woodResources - woodCost);
            playerSO.stoneResources = Mathf.Max(0, playerSO.stoneResources - rockCost);
            playerSO.farmResources = Mathf.Max(0, playerSO.farmResources - farmCost);
            playerSO.coins = Mathf.Max(0, playerSO.coins - coinCost);
            level++;
            changeUpgradeCost();
        }
    }

    public void changeUpgradeCost()
    {
        //it will cost 50% of the original cost at level 1
        if (level == 1)
        {
            woodCost = Mathf.RoundToInt(woodCost * 0.5f);
            rockCost = Mathf.RoundToInt(rockCost * 0.5f);
            farmCost = Mathf.RoundToInt(farmCost * 0.5f);
            coinCost = Mathf.RoundToInt(coinCost * 0.5f);
            increaseStats();
        }
        //it will cost 100% of the original cost at level 2
        else if (level == 2) 
        {
            woodCost = buildingData.woodCost;
            rockCost = buildingData.rockCost;
            farmCost = buildingData.farmCost;
            coinCost = buildingData.coinCost;
            increaseStats();
        }

        //it will increase by 10% of the original cost for each level above 2
        else if (level > 2)
        {
            woodCost = Mathf.RoundToInt(buildingData.woodCost * (1 + (level - 2) * 0.1f));
            rockCost = Mathf.RoundToInt(buildingData.rockCost * (1 + (level - 2) * 0.1f));
            farmCost = Mathf.RoundToInt(buildingData.farmCost * (1 + (level - 2) * 0.1f));
            coinCost = Mathf.RoundToInt(buildingData.coinCost * (1 + (level - 2) * 0.1f));
            increaseStats();
        }
    }

    public void increaseStats()
    {
        // Doubling resourceAmount every level with no cap will eventually overflow a
        // 32-bit int and silently wrap around to a large negative number - which would
        // then get added straight onto a resource on the very next passive tick. Clamp
        // using long arithmetic so it saturates instead of wrapping.
        long doubled = (long)resourceAmount * 2L;
        if (doubled > int.MaxValue)
        {
            resourceAmount = int.MaxValue;
        }
        else if (doubled < int.MinValue)
        {
            resourceAmount = int.MinValue;
        }
        else
        {
            resourceAmount = (int)doubled;
        }
    }

    public void refundStats()
    {
        playerSO.woodResources += Mathf.RoundToInt(woodCost * 0.5f);
        playerSO.stoneResources += Mathf.RoundToInt(rockCost * 0.5f);
        playerSO.farmResources += Mathf.RoundToInt(farmCost * 0.5f);
        playerSO.coins += Mathf.RoundToInt(coinCost * 0.5f);
        playerSO.energyPoints += energyCost;
    }

    public PassiveResourceSaveData CaptureSaveData()
    {
        return new PassiveResourceSaveData
        {
            level = level,
            isActive = isActive,
            resourceAmount = resourceAmount,
            currentTime = currentTime,
            coinCost = coinCost,
            farmCost = farmCost,
            rockCost = rockCost,
            woodCost = woodCost,
            gemCost = gemCost,
            energyCost = energyCost
        };
    }

    public void ApplySaveData(PassiveResourceSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        hasLoadedSavedState = true;
        level = saveData.level;
        isActive = saveData.isActive;
        resourceAmount = saveData.resourceAmount;
        currentTime = saveData.currentTime;
        coinCost = saveData.coinCost;
        farmCost = saveData.farmCost;
        rockCost = saveData.rockCost;
        woodCost = saveData.woodCost;
        gemCost = saveData.gemCost;
        energyCost = saveData.energyCost;
    }
}