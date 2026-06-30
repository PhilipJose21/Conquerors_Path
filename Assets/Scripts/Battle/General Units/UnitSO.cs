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
    public BuildingData buildingData;
    public string unitName;
    public UnitType unitType;
    public int level;
    public int health;
    public int damage;
    public int attackRange;
    public int mobility;
    public int movePoints;
    public int attackPoints;
    public int unitCost;
    public int harvestAmount;
    public Sprite unitIcon;
    public GameObject unitPrefab;
    public GameObject unitButtonPrefab;

    private void OnValidate()
    {
        if (level <= 0)
        {
            level = 1;
        }
    }
}
