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

    [Header("Building Identity")]
    public string buildingName; 
    [TextArea(2, 5)] public string description; 

    [Header("Resource Settings")]
    public ResourceType resourceType; 
    public int resourceAmount; 
    private int totalResourceAmount;
    public float resourceTimer;
}