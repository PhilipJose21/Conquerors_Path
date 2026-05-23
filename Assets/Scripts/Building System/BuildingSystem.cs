using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BuildingSystem : MonoBehaviour
{
    public const float CellSize = 1f;

    public int buildIndex = 0;
    [SerializeField] private List<BuildingData> buildingDatas;

    [SerializeField] private BuildingPreview buildingGrid;

    [SerializeField] private Building buildingPrefab;

    [SerializeField] private BuildingGrid grid;

    private BuildingPreview preview;

    private void Update()
    {
        Vector3 mousePos = GetMouseWorldPosition();
        // Number keys select or change the preview type regardless of preview existing
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) TrySelectBuildIndex(0, mousePos);
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) TrySelectBuildIndex(1, mousePos);
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) TrySelectBuildIndex(2, mousePos);
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) TrySelectBuildIndex(3, mousePos);

        if (preview != null)
        {
            HandlePreview(mousePos);
        }
    }

    public void TrySelectBuildIndex(int index, Vector3 mousePos)
    {
        if (buildingDatas == null || index < 0 || index >= buildingDatas.Count) return;
        buildIndex = index;
        if (preview == null)
        {
            preview = CreatePreview(buildingDatas[buildIndex], mousePos);
        }
        else
        {
            preview.Setup(buildingDatas[buildIndex]);
        }
    }

    private void HandlePreview(Vector3 mouseWorldPosition)
    {
        preview.transform.position = mouseWorldPosition;
        List<Vector3> buildPosition = preview.BuildingModel.GetAllBuildingPosition();
        bool canBuild = grid.CanBuild(buildPosition);
        if (canBuild)
        {
            preview.transform.position = GetSnappedCenterPosition(buildPosition);
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
        if(Input.GetKeyDown(KeyCode.R))
        {
            preview.Rotate(90);
            Debug.Log("Rotated");
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Destroy(preview.gameObject);
            preview = null;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            DeleteBuilding();
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

    private void DeleteBuilding()
    {
        
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
