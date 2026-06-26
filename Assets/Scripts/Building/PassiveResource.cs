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
            resourceAmount = buildingStatsSO.resourceAmount;
            resourceTimer = buildingStatsSO.resourceTimer;
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
        switch (type)
        {
            case BuildingStatsSO.ResourceType.Wood:
                playerSO.woodResources += resourceAmount;
                break;
            case BuildingStatsSO.ResourceType.Stone:
                playerSO.stoneResources += resourceAmount;
                break;
            case BuildingStatsSO.ResourceType.Farm:
                playerSO.farmResources += resourceAmount;
                break;
            case BuildingStatsSO.ResourceType.Energy:
                playerSO.energyPoints += resourceAmount;
                break;
            case BuildingStatsSO.ResourceType.Research:
                playerSO.researchPoints += resourceAmount;
                break;
            case BuildingStatsSO.ResourceType.Gems:
                playerSO.gems += resourceAmount;
                break;
            case BuildingStatsSO.ResourceType.Coins:
                playerSO.coins += resourceAmount;
                break;
        }

        // Track cumulative amount awarded (useful for stats/debugging)
        totalResourceAmount += resourceAmount;
    }

    public void upgradeBuilding()
    {
        Debug.Log("Attempting to upgrade building...");
        if (playerSO.woodResources >= coinCost && playerSO.stoneResources >= rockCost && playerSO.farmResources >= farmCost && playerSO.coins >= coinCost)
        {
            playerSO.woodResources -= woodCost;
            playerSO.stoneResources -= rockCost;
            playerSO.farmResources -= farmCost;
            playerSO.coins -= coinCost;
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
        resourceAmount *= 2;
    }
}
