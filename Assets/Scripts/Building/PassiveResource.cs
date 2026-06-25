using UnityEngine;

public class PassiveResource : MonoBehaviour
{
    // Component that periodically awards a resource to the player while active.
    // It increments a PlayerSO resource field every `resourceTimer` seconds.
    public BuildingStatsSO.ResourceType resourceType;
    public PlayerData playerData => UnityEngine.Object.FindFirstObjectByType<PlayerData>();
    public PlayerSO playerSO => playerData.playerSO;

    private BuildingStatsSO buildingStatsSO;
    public bool isActive;
    public int resourceAmount;
    private int totalResourceAmount;
    public float resourceTimer;
    public float currentTime;

    void Awake()
    {
        buildingStatsSO = GetComponent<BuildingStatContainer>()?.buildingStatsSO;
        if (buildingStatsSO != null)
        {
            resourceType = buildingStatsSO.resourceType;
            resourceAmount = buildingStatsSO.resourceAmount;
            resourceTimer = buildingStatsSO.resourceTimer;
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
}
