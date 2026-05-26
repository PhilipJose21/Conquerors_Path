using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BuildingSystem : MonoBehaviour
{
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
        Vector3 mousePos = GetMouseWorldPosition();

        // Always listen for number key presses so the player can switch selection
        // even while a preview is active.
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TrySelectBuilding(0, mousePos);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TrySelectBuilding(1, mousePos);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TrySelectBuilding(2, mousePos);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TrySelectBuilding(3, mousePos);
        }

        if (preview != null)
        {
            HandlePreview(mousePos);
        }
    }

    private void TrySelectBuilding(int index, Vector3 position)
    {
        if (buildingDataList == null || index < 0 || index >= buildingDataList.Count) return;
        buildingDataIndex = index;
        if (preview != null)
        {
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

    private void HandlePreview(Vector3 mouseWorldPosition)
    {
        preview.transform.position = mouseWorldPosition;
        List<Vector3> buildPosition = preview.BuildingModel.GetAllBuildingPosition();
        bool canBuild = true;
        // Validate each cell against whichever grid contains that world position
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
            // Use the grid that contains the first position for snapping
            BuildingGrid primaryGrid = BuildingGridManager.Instance.FindGridAtPosition(buildPosition.First()) ?? grid;
            // Align preview rotation with the grid so placement and preview match grid orientation
            if (primaryGrid != null)
            {
                preview.transform.rotation = primaryGrid.transform.rotation;
            }
            preview.transform.position = GetSnappedCenterPosition(buildPosition, primaryGrid);
            // Recompute positions after snapping so they match the preview's final transform
            buildPosition = preview.BuildingModel.GetAllBuildingPosition();
            preview.ChangeState(BuildingPreview.BuildingPreviewState.VALID);
            if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.Space))
            {
                PlaceBuilding(buildPosition, null);
            }
        }
        else
        {
            preview.ChangeState(BuildingPreview.BuildingPreviewState.INVALID);
        }
        if(Input.GetKeyDown(KeyCode.R) && canRotate)
        {
            preview.Rotate(90);
            Debug.Log("Preview rotated");
        }
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
