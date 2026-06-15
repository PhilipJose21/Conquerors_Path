using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BuildingSystem : MonoBehaviour
{
    public const float CellSize = 1f;
    // -1 means no building type selected; requires explicit selection to start placing.
    public int buildingDataIndex = -1;

    public bool canRotate = true;

    public bool isPlacing;

    public bool isBattleScene = false;

    public bool enableReinforcementCost = false;

    [SerializeField] private List<BuildingData> buildingDataList;

    [SerializeField] private BuildingPreview buildingGrid;

    [SerializeField] private Building buildingPrefab;

    [SerializeField] private BuildingGrid grid; // fallback or default grid

    [SerializeField] private GameObject environmentParent;

    private BuildingPreview preview;

    private void Awake()
    {
        PlayerBattleSO battleSO = Object.FindFirstObjectByType<PlayerData>()?.playerBattleSO;
        if (battleSO != null)        
        {
            buildingDataList = battleSO.playerUnits.ToList();
        }
    }

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
        // 1. Guard Clause: Verify index bounds before anything else
        if (buildingDataList == null || index < 0 || index >= buildingDataList.Count) 
        {
            return; // Exits early, keeping isPlacing unchanged
        }

        // 2. TOGGLE FEATURE: If the same index is selected again, cancel/deselect placement
        if (isPlacing && buildingDataIndex == index)
        {
            CancelPlacement();
            return;
        }

        isPlacing = true;
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
        // Notify UI manager about the selected building (if present)
        // KingdomUIManager.Instance?.ShowSelectedBuilding(buildingDataList[buildingDataIndex]);
    }

    // Public entry for UI buttons to select a building by index.
    public void SelectBuilding(int index)
    {
        TrySelectBuilding(index, GetMouseWorldPosition());
    }

    // Centralized method to safely clear and cancel placement mode
    public void CancelPlacement()
    {
        if (preview != null)
        {
            Destroy(preview.gameObject);
            preview = null;
        }
        isPlacing = false;
        buildingDataIndex = -1;
        KingdomUIManager.Instance?.CloseObjectInfo();
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
            if (Input.GetMouseButtonDown(0) 
                && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() 
                && !Input.GetKey(KeyCode.Space) 
                && !Input.GetKey(KeyCode.R) 
                && !Input.GetKey(KeyCode.Q)
                )
            {
                if (isBattleScene)
                {
                    if (enableReinforcementCost)
                    {
                        int unitRefCost = buildingDataList[buildingDataIndex].reinforcementCost;
                        ReinforcementCostUpdate costUpdate = Object.FindFirstObjectByType<ReinforcementCostUpdate>();
                        if (costUpdate != null)
                        {
                            TurnManager turnManagerScript = Object.FindFirstObjectByType<TurnManager>();
                            if (costUpdate.unitReinforcementCost >= unitRefCost 
                                && turnManagerScript != null 
                                && turnManagerScript.currentTurnPhase == turnPhase.PlayerTurn)
                            {
                                costUpdate.unitReinforcementCost -= unitRefCost;
                            }
                            else
                            {
                                Debug.Log("Not enough reinforcement cost to place this building.");
                                return;
                            }
                        }
                    }
                    
                    buildingDataList.Remove(preview.Data);
                    Object.FindFirstObjectByType<UnitButtonManager>()?.RefreshUnitButtons();
                }

                PlaceBuilding(buildPosition, primaryGrid);
                // After placing, require the player to explicitly reselect a building
                buildingDataIndex = -1;
                isPlacing = false;
                // Close any selected-building UI
                KingdomUIManager.Instance?.CloseObjectInfo();
                
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

        // Cancel preview via keybind
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CancelPlacement();
        }
    }

    private void PlaceBuilding(List<Vector3> buildPosition, BuildingGrid targetGrid)
    {
        // Determine which grid to use for final placement
        BuildingGrid primaryGrid = BuildingGridManager.Instance.FindGridAtPosition(buildPosition.First()) ?? targetGrid ?? grid;

        // Compute the exact snapped center in world space if we have a grid; otherwise use the preview position
        Vector3 placePosition = preview != null ? preview.transform.position : Vector3.zero;
        Quaternion placeRotation = preview != null ? preview.transform.rotation : Quaternion.identity;
        if (primaryGrid != null)
        {
            placePosition = GetSnappedCenterPosition(buildPosition, primaryGrid);
            placeRotation = primaryGrid.transform.rotation;
        }

        Building building = Instantiate(buildingPrefab, placePosition, placeRotation, environmentParent.transform);
        building.SetUp(preview.Data, preview.BuildingModel.Rotation);

        // Ensure setup didn't shift the transform unexpectedly
        building.transform.SetPositionAndRotation(placePosition, placeRotation);

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
        isPlacing = false;
        buildingDataIndex = -1;
        // Extra cleanup: remove any lingering previews that might have been left behind
        var lingering = Object.FindObjectsByType<BuildingPreview>(FindObjectsSortMode.None);
        foreach (var lp in lingering)
        {
            if (lp != null) Destroy(lp.gameObject);
        }
        PassiveResource passiveResource = building.GetComponentInChildren<PassiveResource>();
        if (passiveResource != null)
        {
            passiveResource.isActive = true;
            passiveResource.currentTime = 0f;
        }

        // After placing, ensure any selected-building UI is closed
        KingdomUIManager.Instance?.CloseObjectInfo();
    }

    // Expose building data for UI code that wants to show costs
    public BuildingData GetBuildingData(int index)
    {
        if (buildingDataList == null) return null;
        if (index < 0 || index >= buildingDataList.Count) return null;
        return buildingDataList[index];
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
        var previewGO = Instantiate(buildingGrid.gameObject, position, Quaternion.identity);
        BuildingPreview newPreview = previewGO.GetComponent<BuildingPreview>();
        newPreview.Setup(data);
        return newPreview;
    }

    public void SelectBuildingByData(BuildingData data)
    {
        if (buildingDataList == null || data == null) return;
        int index = buildingDataList.IndexOf(data);
        if (index >= 0)
        {
            TrySelectBuilding(index, GetMouseWorldPosition());
        }
    }

    public List<BuildingData> GetLiveBuildings()
    {
        return buildingDataList;
    }
}