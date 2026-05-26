using UnityEngine;
using System.Collections.Generic;


public class BuildingGrid : MonoBehaviour
{
    // Represents a rectangular grid area that can contain buildings.
    // Coordinates and cell calculations are performed in the grid's local space
    // so the grid can be rotated and positioned arbitrarily in the world.
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize = BuildingSystem.CellSize;
    private BuildingGridCell[,] grid;

    private void Start()
    {
        grid = new BuildingGridCell[width, height];
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                grid[x, y] = new();
            }
        }
    }

    private void OnEnable()
    {
        BuildingGridManager.Instance?.RegisterGrid(this);
    }

    private void OnDisable()
    {
        BuildingGridManager.Instance?.UnregisterGrid(this);
    }

    public void SetBuilding(Building building, List<Vector3> allBuildingPositions)
    {
        // Mark each covered cell as occupied by the provided Building instance.
        foreach (var position in allBuildingPositions)
        {
            (int x, int y) = WorldToGridPosition(position);
            grid[x, y].SetBuilding(building);
        }
    }

    public bool CanBuild(List<Vector3> allBuildingPositions)
    {
        // Verify all positions fall inside the grid and reference empty cells.
        foreach (var position in allBuildingPositions)
        {
            (int x, int y) = WorldToGridPosition(position);
            if (x < 0 || x >= width || y < 0 || y >= height) return false;
            if (!grid[x, y].IsEmpty()) return false;
        }
        return true;
    }

    public (int x, int y) WorldToGridPosition(Vector3 worldPosition)
    {
        // Convert a world-space point into the grid's integer cell coordinates.
        // Uses the transform's inverse to handle rotated/scaled grids.
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);
        int x = Mathf.FloorToInt(localPos.x / cellSize);
        int y = Mathf.FloorToInt(localPos.z / cellSize);
        return (x, y);
    }

    public bool IsCellEmptyAtWorldPosition(Vector3 worldPosition)
    {
        // Quick check whether the world position maps to an empty grid cell.
        if (!ContainsWorldPosition(worldPosition)) return false;
        (int x, int y) = WorldToGridPosition(worldPosition);
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        return grid[x, y].IsEmpty();
    }

    public void SetBuildingAtWorldPosition(Building building, Vector3 worldPosition)
    {
        // Set the given cell (identified by world position) as occupied by the building.
        if (!ContainsWorldPosition(worldPosition)) return;
        (int x, int y) = WorldToGridPosition(worldPosition);
        if (x < 0 || x >= width || y < 0 || y >= height) return;
        grid[x, y].SetBuilding(building);
    }

    public float CellSize => cellSize;

    public bool ContainsWorldPosition(Vector3 worldPosition)
    {
        // Check if a world-space point falls within the grid bounds expressed in local space.
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);
        float xMin = 0f;
        float xMax = width * cellSize;
        float zMin = 0f;
        float zMax = height * cellSize;
        return localPos.x >= xMin && localPos.x < xMax && localPos.z >= zMin && localPos.z < zMax;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        if (cellSize <= 0 || width <= 0 || height <= 0) return;
        Vector3 origin = transform.position;
        Vector3 right = transform.right * cellSize * transform.lossyScale.x;
        Vector3 forward = transform.forward * cellSize * transform.lossyScale.z;
        Vector3 upOffset = Vector3.up * 0.01f;

        for (int y = 0; y <= height; y++)
        {
            Vector3 start = origin + forward * y + upOffset;
            Vector3 end = origin + forward * y + right * width + upOffset;
            Gizmos.DrawLine(start, end);
        }
        for (int x = 0; x <= width; x++)
        {
            Vector3 start = origin + right * x + upOffset;
            Vector3 end = origin + right * x + forward * height + upOffset;
            Gizmos.DrawLine(start, end);
        }
    }
}

public class BuildingGridCell
{
    private Building building;

    public void SetBuilding(Building building)
    {
        this.building = building;
    }

    public bool IsEmpty()
    {
        return building == null;
    }
}
