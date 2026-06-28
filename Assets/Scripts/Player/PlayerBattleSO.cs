using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu()]
public class PlayerBattleSO : ScriptableObject
{
    public List<BuildingData> playerUnits;
    public List<UnitSO> playerUnitStats;
    public List<GameObject> playerUnitPrefabs;
    public int playerReinforcementCost;
    public int woodHarvestAmount;
    public int stoneHarvestAmount;
    public int farmHarvestAmount;
    public int goldHarvestAmount;

    public void EnsureRuntimeLists()
    {
        playerUnits ??= new List<BuildingData>();
        playerUnitStats ??= new List<UnitSO>();
        playerUnitPrefabs ??= new List<GameObject>();

        while (playerUnitPrefabs.Count < playerUnitStats.Count)
        {
            UnitSO unit = playerUnitStats[playerUnitPrefabs.Count];
            playerUnitPrefabs.Add(unit != null ? unit.unitPrefab : null);
        }

        while (playerUnitStats.Count < playerUnitPrefabs.Count)
        {
            playerUnitStats.Add(null);
        }
    }
}
