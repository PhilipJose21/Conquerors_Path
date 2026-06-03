using UnityEngine;

public class PassiveResource : MonoBehaviour
{
    // Component that periodically awards a resource to the player while active.
    // It increments a PlayerSO resource field every `resourceTimer` seconds.
    public enum ResourceType
    {
        Wood,
        Stone,
        Farm,
        Energy,
        Research,
        Gems,
        Coins
    }

    public ResourceType resourceType;
    public PlayerData playerData => UnityEngine.Object.FindFirstObjectByType<PlayerData>();
    public PlayerSO playerSO => playerData.playerSO;

    public bool isActive;
    public int resourceAmount;
    private int totalResourceAmount;
    public float resourceTimer;
    public float currentTime;


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

    

    public void AddResource(ResourceType type)
    {
        // Add the configured resource amount to the player's SO based on type.
        switch (type)
        {
            case ResourceType.Wood:
                playerSO.woodResources += resourceAmount;
                break;
            case ResourceType.Stone:
                playerSO.stoneResources += resourceAmount;
                break;
            case ResourceType.Farm:
                playerSO.farmResources += resourceAmount;
                break;
            case ResourceType.Energy:
                playerSO.energyPoints += resourceAmount;
                break;
            case ResourceType.Research:
                playerSO.researchPoints += resourceAmount;
                break;
            case ResourceType.Gems:
                playerSO.gems += resourceAmount;
                break;
            case ResourceType.Coins:
                playerSO.coins += resourceAmount;
                break;
        }

        // Track cumulative amount awarded (useful for stats/debugging)
        totalResourceAmount += resourceAmount;
    }
}
