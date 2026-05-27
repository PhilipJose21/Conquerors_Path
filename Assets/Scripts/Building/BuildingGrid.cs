using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class BuildingGrid : MonoBehaviour
{
    // Represents a rectangular grid area that can contain buildings.
    // Coordinates and cell calculations are performed in the grid's local space
    // so the grid can be rotated and positioned arbitrarily in the world.
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize = BuildingSystem.CellSize;
    private BuildingGridCell[,] grid;
    private Material lineMaterial;
    [SerializeField] private Color lineColor = Color.white;

    BuildingSystem buildingSystem;
    public bool showGrid = false;

    private void Awake()
    {
        if (lineMaterial == null)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.SetInt("_ZWrite", 0);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        }
        // Find the BuildingSystem component in the scene (fallback: use tag lookup if needed)
        buildingSystem = FindObjectOfType<BuildingSystem>();
    }

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

        // On start, mark any existing buildings or building-models that were
        // placed into the scene (for example dragged in from the prefab folder)
        // so they occupy their grid cells during Play mode.
        // 1) Register real `Building` instances.
        var existingBuildings = FindObjectsOfType<Building>();
        foreach (var b in existingBuildings)
        {
            var model = b.GetComponentInChildren<BuildingModel>();
            if (model == null) continue;
            var positions = model.GetAllBuildingPosition();
            if (positions != null && positions.Count > 0)
                SetBuilding(b, positions);
        }

        // 2) Also handle standalone `BuildingModel` objects that were placed
        // directly in the scene without a `Building` component by adding a
        // lightweight `Building` marker at runtime so the grid can track them.
        var allModels = FindObjectsOfType<BuildingModel>();
        foreach (var m in allModels)
        {
            if (m.GetComponentInParent<Building>() != null) continue;
            Building marker = m.gameObject.GetComponent<Building>();
            if (marker == null) marker = m.gameObject.AddComponent<Building>();
            var positions = m.GetAllBuildingPosition();
            if (positions != null && positions.Count > 0)
                SetBuilding(marker, positions);
        }
    }

    void Update()
    {
        // Follow the BuildingSystem's placing state rather than overwriting it.
        if (buildingSystem != null)
        {
            showGrid = buildingSystem.isPlacing;
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
        if (!showGrid) return;
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

    void OnRenderObject()
    {
        if (!showGrid) return;
        if (lineMaterial == null) return;
        lineMaterial.SetPass(0);

        GL.PushMatrix();
        // Draw in the grid's local space
        GL.MultMatrix(transform.localToWorldMatrix);

        GL.Begin(GL.LINES);
        GL.Color(lineColor);

        // Horizontal lines (along local z)
        for (int y = 0; y <= height; y++)
        {
            float z = y * cellSize;
            GL.Vertex3(0f, 0.01f, z);
            GL.Vertex3(width * cellSize, 0.01f, z);
        }

        // Vertical lines (along local x)
        for (int x = 0; x <= width; x++)
        {
            float xPos = x * cellSize;
            GL.Vertex3(xPos, 0.01f, 0f);
            GL.Vertex3(xPos, 0.01f, height * cellSize);
        }

        GL.End();
        GL.PopMatrix();
    }

    void OnDestroy()
    {
        if (lineMaterial) Destroy(lineMaterial);
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
