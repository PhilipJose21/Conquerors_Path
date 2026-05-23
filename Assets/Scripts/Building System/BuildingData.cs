using UnityEngine;

[CreateAssetMenu(menuName = "Data/Building")]
public class BuildingData : ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; } 
    [field: SerializeField] public int CellSize { get; private set; } //Cell Size
    [field: SerializeField] public BuildingModel Model { get; private set; }
}