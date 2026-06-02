using UnityEngine;

[CreateAssetMenu()]
public class UnitSO : ScriptableObject
{

    public enum UnitType
    {
        Melee,
        Ranger,
        Support
    }

    public string unitName;
    public UnitType unitType;
    public int health;
    public int damage;
    public int attackRange;
    public int mobility;
    public int movePoints;
    public int attackPoints;
    public int unitCost;
    public Sprite unitIcon;
    public GameObject unitPrefab;
}
