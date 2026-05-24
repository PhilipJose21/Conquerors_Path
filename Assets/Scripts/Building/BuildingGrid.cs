using UnityEngine;
using System.Collections.Generic;


public class BuildingGrid : MonoBehaviour
{
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
        foreach (var position in allBuildingPositions)
        {
            (int x, int y) = WorldToGridPosition(position);
            grid[x, y].SetBuilding(building);
        }
    }

    public bool CanBuild(List<Vector3> allBuildingPositions)
    {
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
        int x = Mathf.FloorToInt((worldPosition - transform.position).x / cellSize);
        int y = Mathf.FloorToInt((worldPosition - transform.position).z / cellSize);
        return (x, y);
    }

    public bool IsCellEmptyAtWorldPosition(Vector3 worldPosition)
    {
        if (!ContainsWorldPosition(worldPosition)) return false;
        (int x, int y) = WorldToGridPosition(worldPosition);
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        return grid[x, y].IsEmpty();
    }

    public void SetBuildingAtWorldPosition(Building building, Vector3 worldPosition)
    {
        if (!ContainsWorldPosition(worldPosition)) return;
        (int x, int y) = WorldToGridPosition(worldPosition);
        if (x < 0 || x >= width || y < 0 || y >= height) return;
        grid[x, y].SetBuilding(building);
    }

    public float CellSize => cellSize;

    public bool ContainsWorldPosition(Vector3 worldPosition)
    {
        Vector3 origin = transform.position;
        float xMin = origin.x;
        float xMax = origin.x + width * cellSize;
        float zMin = origin.z;
        float zMax = origin.z + height * cellSize;
        return worldPosition.x >= xMin && worldPosition.x < xMax && worldPosition.z >= zMin && worldPosition.z < zMax;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        if (cellSize <= 0 || width <= 0 || height <= 0) return;
        Vector3 origin = transform.position;
        for (int y = 0; y <= height; y++)
        {
            Vector3 start = origin + new Vector3(0, 0.01f, y * cellSize);
            Vector3 end = origin + new Vector3(width * cellSize, 0.01f, y * cellSize);
            Gizmos.DrawLine(start, end);
        }
        for (int x = 0; x <= width; x++)
        {
            Vector3 start = origin + new Vector3(x * cellSize, 0.01f, 0);
            Vector3 end = origin + new Vector3(x * cellSize, 0.01f, height * cellSize);
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
