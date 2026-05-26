using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BuildingSystem : MonoBehaviour
{
    // Manages building previews, validation and placement.
    // - Creates and positions BuildingPreview instances
    // - Validates preview cell occupancy against registered BuildingGrid instances
    // - Snaps previews to the grid-local center and instantiates Building objects
    public const float CellSize = 1f;
    public int buildingDataIndex = 0;

    public bool canRotate = true;

    [SerializeField] private List<BuildingData> buildingDataList;

    [SerializeField] private BuildingPreview buildingGrid;

    [SerializeField] private Building buildingPrefab;

    [SerializeField] private BuildingGrid grid; // fallback or default grid

    [SerializeField] private GameObject environmentParent;

    private BuildingPreview preview;

    private void Update()
    {
        // Check input and update the active preview each frame.
        Vector3 mousePos = GetMouseWorldPosition();

        // Number keys switch selected building type even while previewing.
        if (Input.GetKeyDown(KeyCode.Alpha1)) TrySelectBuilding(0, mousePos);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) TrySelectBuilding(1, mousePos);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) TrySelectBuilding(2, mousePos);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) TrySelectBuilding(3, mousePos);

        // If a preview exists, move and validate it.
        if (preview != null) HandlePreview(mousePos);
    }

    // Select a building by index and (re)create the preview at the given position.
    private void TrySelectBuilding(int index, Vector3 position)
    {
        if (buildingDataList == null || index < 0 || index >= buildingDataList.Count) return;
        buildingDataIndex = index;
        if (preview != null)
        {
            // Preserve previous preview position when switching building types
            Vector3 prevPos = preview.transform.position;
            Destroy(preview.gameObject);
            preview = CreatePreview(buildingDataList[buildingDataIndex], prevPos);
        }
        else
        {
            preview = CreatePreview(buildingDataList[buildingDataIndex], position);
        }
    }

    // Public entry for UI buttons to select a building by index.
    public void SelectBuilding(int index)
    {
        TrySelectBuilding(index, GetMouseWorldPosition());
    }

    // Update preview position, validate against grids and snap/place when appropriate.
    private void HandlePreview(Vector3 mouseWorldPosition)
    {
        // Move preview to follow mouse
        preview.transform.position = mouseWorldPosition;
        // Gather all world positions for the preview's building units
        List<Vector3> buildPosition = preview.BuildingModel.GetAllBuildingPosition();
        bool canBuild = true;

        // Validate each unit's world position against whichever grid contains it
        foreach (var pos in buildPosition)
        {
            BuildingGrid posGrid = BuildingGridManager.Instance.FindGridAtPosition(pos);
            if (posGrid == null || !posGrid.IsCellEmptyAtWorldPosition(pos))
            {
                canBuild = false;
                break;
            }
        }

        if (canBuild)
        {
            // Choose primary grid for snapping and orientation
            BuildingGrid primaryGrid = BuildingGridManager.Instance.FindGridAtPosition(buildPosition.First()) ?? grid;
            if (primaryGrid != null)
            {
                // Align preview rotation to the grid's rotation so visuals match
                preview.transform.rotation = primaryGrid.transform.rotation;
            }
            // Snap preview to the center of occupied cells in grid local space
            preview.transform.position = GetSnappedCenterPosition(buildPosition, primaryGrid);
            // Recompute positions after snapping so validation/placement uses final transforms
            buildPosition = preview.BuildingModel.GetAllBuildingPosition();
            preview.ChangeState(BuildingPreview.BuildingPreviewState.VALID);

            // Place building on left mouse click (unless holding Space for camera)
            if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.Space))
            {
                PlaceBuilding(buildPosition, null);
            }
        }
        else
        {
            preview.ChangeState(BuildingPreview.BuildingPreviewState.INVALID);
        }

        // Rotation input for the preview
        if (Input.GetKeyDown(KeyCode.R) && canRotate)
        {
            preview.Rotate(90);
        }

        // Cancel preview
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Destroy(preview.gameObject);
            preview = null;
        }
    }

    private void PlaceBuilding(List<Vector3> buildPosition, BuildingGrid targetGrid)
    {
        Building building = Instantiate(buildingPrefab, preview.transform.position, Quaternion.identity, environmentParent.transform);
        building.SetUp(preview.Data, preview.BuildingModel.Rotation);
        // Assign each position to its containing grid
        foreach (var pos in buildPosition)
        {
            BuildingGrid posGrid = BuildingGridManager.Instance.FindGridAtPosition(pos) ?? targetGrid ?? grid;
            if (posGrid != null)
            {
                posGrid.SetBuildingAtWorldPosition(building, pos);
            }
            else
            {
                Debug.LogWarning("No grid found for position " + pos + " — skipping cell assignment.");
            }
        }
        Destroy(preview.gameObject);
        preview = null;
        PassiveResource passiveResource = building.GetComponentInChildren<PassiveResource>();
        if (passiveResource != null)
        {
            passiveResource.isActive = true;
            passiveResource.currentTime = 0f;
        }
    }

    private Vector3 GetSnappedCenterPosition(List<Vector3> allBuildingPositions, BuildingGrid targetGrid)
    {
        if (targetGrid == null) targetGrid = grid;
        float cs = targetGrid.CellSize;
        List<int> xs = new List<int>();
        List<int> zs = new List<int>();
        foreach (var p in allBuildingPositions)
        {
            (int gx, int gz) = targetGrid.WorldToGridPosition(p);
            xs.Add(gx);
            zs.Add(gz);
        }
        int minX = xs.Min();
        int maxX = xs.Max();
        int minZ = zs.Min();
        int maxZ = zs.Max();
        float centerLocalX = (maxX + minX) / 2f * cs + cs / 2f;
        float centerLocalZ = (maxZ + minZ) / 2f * cs + cs / 2f;
        Vector3 centerLocal = new Vector3(centerLocalX, 0f, centerLocalZ);
        Vector3 worldCenter = targetGrid.transform.TransformPoint(centerLocal);
        // Preserve the grid's y position
        worldCenter.y = targetGrid.transform.position.y;
        return worldCenter;
    }
    
    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    private BuildingPreview CreatePreview(BuildingData data, Vector3 position)
    {
        BuildingPreview newPreview = Instantiate(buildingGrid, position, Quaternion.identity);
        newPreview.Setup(data);
        return newPreview;
    }

}
