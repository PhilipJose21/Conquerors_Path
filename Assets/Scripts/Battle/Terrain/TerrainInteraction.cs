using UnityEngine;

public class TerrainInteraction : MonoBehaviour
{
    private TerrainSO terrainData;

    public bool disruptsMovement;
    public bool attackRangeImmune;
    public bool unitVisibility;
    public bool cannotMoveOn;
    void Start()
    {
        TerrainSOContainer container = GetComponent<TerrainSOContainer>();
        terrainData = container.terrainData;

        disruptsMovement = terrainData.disruptsMovement;
        attackRangeImmune = terrainData.attackRangeImmune;
        unitVisibility = terrainData.unitVisibility;
        cannotMoveOn = terrainData.cannotMoveOn;

        if (terrainData.terrainDamage > 0)
        {
            this.gameObject.AddComponent<TerrainDamage>();
        }

        if (terrainData.unitVisibility)
        {
            this.gameObject.AddComponent<TerrainHideUnit>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Return whether this terrain blocks passage (unit must stop on this tile)
    public bool CantWalkThrough()
    {
        return disruptsMovement;
    }

    // Return whether this terrain prevents movement (unit cannot move onto this tile)
    public bool CantMoveOn()
    {
        return cannotMoveOn;
    }

    public bool IsAttackRangeImmune()
    {
        return attackRangeImmune;
    }

    public bool HasUnitVisibility()
    {
        return unitVisibility;
    }


}
