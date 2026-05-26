using System.Collections.Generic;
using UnityEngine;

public class BuildingGridManager
{
    // Simple singleton manager that keeps track of all BuildingGrid instances
    // in the scene. Used to find which grid contains a world position.
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
        // Add a grid to the registry if it's not already present.
        if (!grids.Contains(grid)) grids.Add(grid);
    }

    public void UnregisterGrid(BuildingGrid grid)
    {
        // Remove a grid from the registry when it is disabled/destroyed.
        if (grids.Contains(grid)) grids.Remove(grid);
    }

    public BuildingGrid FindGridAtPosition(Vector3 worldPosition)
    {
        // Return the first registered grid that contains the given world position.
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

    /// <summary>
    /// Enumerate all registered grids. Useful for runtime drawing/debugging.
    /// </summary>
    public IEnumerable<BuildingGrid> GetAllGrids()
    {
        return grids;
    }
}
