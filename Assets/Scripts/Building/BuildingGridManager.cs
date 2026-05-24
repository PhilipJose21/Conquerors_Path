using System.Collections.Generic;
using UnityEngine;

public class BuildingGridManager
{
    private static BuildingGridManager instance;
    public static BuildingGridManager Instance
    {
        get
        {
            if (instance == null) instance = new BuildingGridManager();
            return instance;
        }
    }

    private readonly List<BuildingGrid> grids = new();

    public void RegisterGrid(BuildingGrid grid)
    {
        if (!grids.Contains(grid)) grids.Add(grid);
    }

    public void UnregisterGrid(BuildingGrid grid)
    {
        if (grids.Contains(grid)) grids.Remove(grid);
    }

    public BuildingGrid FindGridAtPosition(Vector3 worldPosition)
    {
        foreach (var g in grids)
        {
            if (g != null && g.ContainsWorldPosition(worldPosition)) return g;
        }
        return null;
    }

    public BuildingGrid FindGridForPositions(List<Vector3> positions)
    {
        if (positions == null || positions.Count == 0) return null;
        // Prefer a grid that contains all positions
        foreach (var g in grids)
        {
            bool allInside = true;
            foreach (var p in positions)
            {
                if (!g.ContainsWorldPosition(p))
                {
                    allInside = false;
                    break;
                }
            }
            if (allInside) return g;
        }
        // Fallback: return the grid containing the first position
        return FindGridAtPosition(positions[0]);
    }
}
