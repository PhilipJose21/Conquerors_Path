using UnityEngine;

[CreateAssetMenu()]
public class PlayerBattleSO : ScriptableObject
{
    public BuildingData[] playerUnits;
    public int playerReinforcementCost;
    public int woodHarvestAmount;
    public int stoneHarvestAmount;
    public int farmHarvestAmount;
    public int goldHarvestAmount;
}
