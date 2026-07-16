using UnityEngine;

[CreateAssetMenu(menuName = "Data/Troop")]
public class TroopData : ScriptableObject
{
    public enum TroopType { Melee, Ranger, Support }

    public UnitSO unitStats;
    public string unitName;
    public TroopType unitType;
    public string description;
    public Sprite unitIcon;

    public int health;
    public int damage;
    [Tooltip("Mobility in grid cells (Manhattan distance).")]
    public int mobility;
    [Tooltip("Attack range in grid cells (square radius).")]
    public int attackRange;
    public int unitCost;

    public bool canUpgrade = false;
    public bool canDestroy = false;

    private void OnEnable()
    {
        SyncUnitStats();
    }

    private void OnValidate()
    {
        SyncUnitStats();
    }

    [ContextMenu("Sync Unit Stats")]
    public void SyncUnitStats()
    {
        if (unitStats == null)
        {
            return;
        }

        unitName = unitStats.unitName;
        unitType = ConvertUnitType(unitStats.unitType);
        health = unitStats.health;
        damage = unitStats.damage;
        attackRange = unitStats.attackRange;
        mobility = unitStats.mobility;
        unitCost = unitStats.unitCost;
    }

    private static TroopType ConvertUnitType(UnitSO.UnitType unitType)
    {
        switch (unitType)
        {
            case UnitSO.UnitType.Ranger:
                return TroopType.Ranger;
            case UnitSO.UnitType.Support:
                return TroopType.Support;
            default:
                return TroopType.Melee;
        }
    }
}
