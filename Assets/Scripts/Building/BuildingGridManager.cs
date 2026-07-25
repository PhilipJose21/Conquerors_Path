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
        // Among every registered grid whose X/Z footprint contains this
        // position, prefer whichever one's height is actually closest to the
        // query — not just the first match in registration order. Grids can
        // legitimately overlap in X/Z at different elevations (e.g. a raised
        // platform next to lower ground), and ContainsWorldPosition only
        // checks X/Z, so without this a query could resolve to a grid at the
        // wrong height even though a better match was also available.
        BuildingGrid best = null;
        float bestYDist = float.MaxValue;
        foreach (var g in grids)
        {
            if (g == null || !g.ContainsWorldPosition(worldPosition)) continue;
            float yDist = Mathf.Abs(g.transform.position.y - worldPosition.y);
            if (yDist < bestYDist)
            {
                bestYDist = yDist;
                best = g;
            }
        }
        return best;
    }

    public BuildingGrid FindGridForPositions(List<Vector3> positions)
    {
        if (positions == null || positions.Count == 0) return null;

        // Prefer a grid that contains all positions - among those, the one
        // whose height is closest to the average of the queried positions.
        BuildingGrid best = null;
        float bestYDist = float.MaxValue;
        foreach (var g in grids)
        {
            if (g == null) continue;
            bool allInside = true;
            float avgY = 0f;
            foreach (var p in positions)
            {
                if (!g.ContainsWorldPosition(p))
                {
                    allInside = false;
                    break;
                }
                avgY += p.y;
            }
            if (!allInside) continue;

            avgY /= positions.Count;
            float yDist = Mathf.Abs(g.transform.position.y - avgY);
            if (yDist < bestYDist)
            {
                bestYDist = yDist;
                best = g;
            }
        }
        if (best != null) return best;

        // Fallback: return the (now also height-aware) grid containing the first position
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