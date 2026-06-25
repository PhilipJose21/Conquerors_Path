using UnityEngine;

[CreateAssetMenu()]
public class BuildingStatsSO : ScriptableObject
{
    public enum ResourceType
    {
        None,
        Wood,
        Stone,
        Farm,
        Energy,
        Research,
        Gems,
        Coins
    }

    public string buildingName;
    public string description;
    public ResourceType resourceType;
    public int resourceAmount;
    private int totalResourceAmount;
    public float resourceTimer;
}
