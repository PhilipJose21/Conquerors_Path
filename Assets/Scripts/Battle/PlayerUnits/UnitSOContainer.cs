using UnityEngine;

public class UnitSOContainer : MonoBehaviour
{
    public UnitSO unitData;

    public int additionalHealth;
    public int additionalDamage;
    public int additionalAttackRange;
    public int additionalMobility;
    public int additionalMovePoints;
    public int additionalAttackPoints;
    public int additionalHarvestAmount;

    public void SetUnitData(UnitSO newUnitData)
    {
        unitData = newUnitData;
    }

    public int GetHealth()
    {
        return unitData != null ? unitData.health + additionalHealth : additionalHealth;
    }

    public int GetDamage()
    {
        return unitData != null ? unitData.damage + additionalDamage : additionalDamage;
    }

    public int GetAttackRange()
    {
        return unitData != null ? unitData.attackRange + additionalAttackRange : additionalAttackRange;
    }

    public int GetMobility()
    {
        return unitData != null ? unitData.mobility + additionalMobility : additionalMobility;
    }

    public int GetMovePoints()
    {
        return unitData != null ? unitData.movePoints + additionalMovePoints : additionalMovePoints;
    }

    public int GetAttackPoints()
    {
        return unitData != null ? unitData.attackPoints + additionalAttackPoints : additionalAttackPoints;
    }

    public int GetHarvestAmount()
    {
        return unitData != null ? unitData.harvestAmount + additionalHarvestAmount : additionalHarvestAmount;
    }
}
