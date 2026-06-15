using UnityEngine;

[CreateAssetMenu(menuName = "Data/Building")]
public class BuildingData : ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public int Size { get; private set; }
    [field: SerializeField] public BuildingModel Model { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public int coinCost { get; private set; }
    [field: SerializeField] public int farmCost { get; private set; }
    [field: SerializeField] public int rockCost { get; private set; }
    [field: SerializeField] public int woodCost { get; private set; }
    [field: SerializeField] public int gemCost { get; private set; }
    [field: SerializeField] public int energyCost { get; private set; }
    [field: SerializeField] public int unitCost { get; private set; }
    [field: SerializeField] public int reinforcementCost { get; private set; }
    [field: SerializeField] public UnitSO unitPrefab { get; private set; }
}
