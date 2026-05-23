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

    [SerializeField] private BuildingGrid grid;

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
        bool canBuild = grid.CanBuild(buildPosition);
        if (canBuild)
        {
            preview.transform.position = GetSnappedCenterPosition(buildPosition);
            // Recompute positions after snapping so they match the preview's final transform
            buildPosition = preview.BuildingModel.GetAllBuildingPosition();
            preview.ChangeState(BuildingPreview.BuildingPreviewState.VALID);
            if (Input.GetMouseButtonDown(0))
            {
                PlaceBuilding(buildPosition);
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

    private void PlaceBuilding(List<Vector3> buildPosition)
    {
        Building building = Instantiate(buildingPrefab, preview.transform.position, Quaternion.identity);
        building.SetUp(preview.Data, preview.BuildingModel.Rotation);
        grid.SetBuilding(building, buildPosition);
        Destroy(preview.gameObject);
        preview = null;
    }

    private Vector3 GetSnappedCenterPosition(List<Vector3> allBuildingPositions)
    {
        Vector3 gridOrigin = grid.transform.position;
        List<int> xs = allBuildingPositions.Select(p => Mathf.FloorToInt((p.x - gridOrigin.x) / CellSize)).ToList();
        List<int> zs = allBuildingPositions.Select(p => Mathf.FloorToInt((p.z - gridOrigin.z) / CellSize)).ToList();
        float centerX = gridOrigin.x + (xs.Max() + xs.Min()) / 2f * CellSize + CellSize / 2f;
        float centerZ = gridOrigin.z + (zs.Max() + zs.Min()) / 2f * CellSize + CellSize / 2f;
        return new Vector3(centerX, gridOrigin.y, centerZ);
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
