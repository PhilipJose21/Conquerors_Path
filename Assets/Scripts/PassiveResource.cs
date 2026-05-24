using UnityEngine;

public class PassiveResource : MonoBehaviour
{
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
    public PlayerData playerData => FindObjectOfType<PlayerData>();
    public PlayerSO playerSO => playerData.playerSO;

    public bool isActive;
    public int resourceAmount;
    private int totalResourceAmount;
    public float resourceTimer;
    public float currentTime;


    void Update()
    {
        if (!isActive)
            return;
        currentTime += Time.deltaTime;

        if (currentTime >= resourceTimer)
        {
            currentTime = 0f;
            AddResource(resourceType);
        }
        
    }

    

    public void AddResource(ResourceType type)
    {
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

        totalResourceAmount += resourceAmount;
    }
}
