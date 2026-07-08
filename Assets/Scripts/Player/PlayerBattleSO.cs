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

    public LevelSO currentLevel;
    public List<LevelSO> unlockedLevels;
    public List<LevelSO> completedLevels;


//THIS PART IS NOT TRUSTED, MAY REMOVE IN THE FUTURE
    [System.NonSerialized] private bool isSyncingLists;
    [System.NonSerialized] private List<BuildingData> previousPlayerUnits;
    [System.NonSerialized] private List<UnitSO> previousPlayerUnitStats;

    private void OnValidate()
    {
        if (isSyncingLists)
        {
            return;
        }

        isSyncingLists = true;
        try
        {
            if (playerUnits == null)
            {
                playerUnits = new List<BuildingData>();
            }

            if (playerUnitStats == null)
            {
                playerUnitStats = new List<UnitSO>();
            }

            if (previousPlayerUnits == null)
            {
                previousPlayerUnits = new List<BuildingData>(playerUnits);
            }

            if (previousPlayerUnitStats == null)
            {
                previousPlayerUnitStats = new List<UnitSO>(playerUnitStats);
            }

            int removedFromUnitsIndex = FindRemovedIndex(previousPlayerUnits, playerUnits);
            int removedFromStatsIndex = FindRemovedIndex(previousPlayerUnitStats, playerUnitStats);

            if (removedFromUnitsIndex >= 0 && playerUnitStats.Count == previousPlayerUnitStats.Count)
            {
                int clampedIndex = Mathf.Clamp(removedFromUnitsIndex, 0, playerUnitStats.Count - 1);
                if (playerUnitStats.Count > 0)
                {
                    playerUnitStats.RemoveAt(clampedIndex);
                }
            }
            else if (removedFromStatsIndex >= 0 && playerUnits.Count == previousPlayerUnits.Count)
            {
                int clampedIndex = Mathf.Clamp(removedFromStatsIndex, 0, playerUnits.Count - 1);
                if (playerUnits.Count > 0)
                {
                    playerUnits.RemoveAt(clampedIndex);
                }
            }

            SyncListCounts();

            previousPlayerUnits = new List<BuildingData>(playerUnits);
            previousPlayerUnitStats = new List<UnitSO>(playerUnitStats);
        }
        finally
        {
            isSyncingLists = false;
        }
    }

    private void SyncListCounts()
    {
        while (playerUnits.Count < playerUnitStats.Count)
        {
            playerUnits.Add(null);
        }

        while (playerUnitStats.Count < playerUnits.Count)
        {
            playerUnitStats.Add(null);
        }
    }

    private static int FindRemovedIndex<T>(List<T> previous, List<T> current) where T : UnityEngine.Object
    {
        if (previous == null || current == null)
        {
            return -1;
        }

        if (previous.Count != current.Count + 1)
        {
            return -1;
        }

        int previousIndex = 0;
        int currentIndex = 0;

        while (previousIndex < previous.Count && currentIndex < current.Count)
        {
            if (previous[previousIndex] == current[currentIndex])
            {
                previousIndex++;
                currentIndex++;
                continue;
            }

            int assumedRemovedIndex = previousIndex;
            previousIndex++;

            while (previousIndex < previous.Count && currentIndex < current.Count)
            {
                if (previous[previousIndex] != current[currentIndex])
                {
                    return -1;
                }

                previousIndex++;
                currentIndex++;
            }

            return assumedRemovedIndex;
        }

        return previous.Count - 1;
    }
}
