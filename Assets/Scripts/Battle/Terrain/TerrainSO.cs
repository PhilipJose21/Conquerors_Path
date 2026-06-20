using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu()]
public class TerrainSO : ScriptableObject
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

    public string terrainName;
    public GameObject terrainPrefab;
    public ResourceType resourceType;
    public ResourceType secondaryResourceType;
    public bool unitVisibility;
    public bool disruptsMovement;
    public bool attackRangeImmune;
    public bool cannotMoveOn;
    public int terrainDamage;
    public string specialConditions;
    
}
