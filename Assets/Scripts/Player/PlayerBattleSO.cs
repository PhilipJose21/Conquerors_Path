using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu()]
public class PlayerBattleSO : ScriptableObject
{
    public List<BuildingData> playerUnits;
    public List<UnitSO> playerUnitStats;
    public int playerReinforcementCost;
    public int woodHarvestAmount;
    public int stoneHarvestAmount;
    public int farmHarvestAmount;
    public int goldHarvestAmount;
}
