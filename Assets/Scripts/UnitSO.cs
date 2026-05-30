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
    public float attackRange;
    public float mobility;
    public int unitCost;
    public Sprite unitIcon;
    public GameObject unitPrefab;
}
