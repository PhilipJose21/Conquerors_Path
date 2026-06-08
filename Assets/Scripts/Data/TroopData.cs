using UnityEngine;

[CreateAssetMenu(menuName = "Data/Troop")]
public class TroopData : ScriptableObject
{
    public enum TroopType { Melee, Ranger, Support }

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

    public bool canUpgrade = true;
    public bool canDestroy = true;
}
