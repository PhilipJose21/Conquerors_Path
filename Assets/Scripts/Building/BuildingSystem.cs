using UnityEngine;
using UnityEngine.EventSystems;
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

    public bool enableRemovingUnitFromArray = false;

    [SerializeField] private List<BuildingData> buildingDataList;

    [SerializeField] private BuildingPreview buildingGrid;

    [SerializeField] private Building buildingPrefab;

    [SerializeField] private BuildingGrid grid; // fallback or default grid

    [SerializeField] private GameObject environmentParent;

    private PlayerData playerData;
    private PlayerSO playerSO;
    private PlayerBattleSO playerBattleSO;

    private BuildingPreview preview;

    // Map of keys to use for quick selection. Extend this array to support more slots.
    private static readonly KeyCode[] numberKeyMap = new KeyCode[]
    {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5,
        KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0,
        KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4, KeyCode.Keypad5,
        KeyCode.Keypad6, KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9, KeyCode.Keypad0,
        KeyCode.F1, KeyCode.F2, KeyCode.F3, KeyCode.F4, KeyCode.F5, KeyCode.F6,
        KeyCode.F7, KeyCode.F8, KeyCode.F9, KeyCode.F10, KeyCode.F11, KeyCode.F12
    };

    private void Awake()
    {
        playerData = Object.FindFirstObjectByType<PlayerData>();
        playerSO = playerData.playerSO;
        playerBattleSO = playerData?.playerBattleSO;
        if (playerBattleSO != null && isBattleScene)        
        {
            buildingDataList = playerBattleSO.playerUnits.ToList();
        }

    }

    private void Update()
    {
        // Check input and update the active preview each frame.
        Vector3 mousePos = GetMouseWorldPosition();

        // Number keys switch selected building type even while previewing.
        if (buildingDataList != null)
        {
            int max = Mathf.Min(buildingDataList.Count, numberKeyMap.Length);
            for (int i = 0; i < max; i++)
            {
                if (Input.GetKeyDown(numberKeyMap[i]))
                {
                    TrySelectBuilding(i, mousePos);
                    break;
                }
            }
        }


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

        
        if (buildingDataList[index].coinCost > playerSO.coins||
            buildingDataList[index].woodCost > playerSO.woodResources||
            buildingDataList[index].rockCost > playerSO.stoneResources||
            buildingDataList[index].farmCost > playerSO.farmResources
            ||buildingDataList[index].energyCost > playerSO.energyPoints)
        {
            Debug.Log("Not enough resources to select this building.");
            return;
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
        // 1. Move preview roughly to follow mouse first
        preview.transform.position = mouseWorldPosition;
        
        // 2. Gather preliminary positions to find the target grid
        List<Vector3> roughPositions = preview.BuildingModel.GetAllBuildingPosition();
        if (roughPositions.Count == 0) return;

        // Choose primary grid for snapping and orientation - prefer a grid that contains the
        // entire footprint to avoid selecting the wrong nearby grid when shape unit order
        // causes the first unit to be outside the intended grid.
        BuildingGrid primaryGrid = BuildingGridManager.Instance.FindGridForPositions(roughPositions) ?? grid;
        
        if (primaryGrid != null)
        {
            // Align preview rotation to the grid's rotation so visual math lines up
            preview.transform.rotation = primaryGrid.transform.rotation;
            
            // CRITICAL FIX: Snap the preview to the final cell positions BEFORE validating
            preview.transform.position = GetSnappedCenterPosition(roughPositions, primaryGrid);
        }

        // 3. NOW gather the final, snapped world positions for precise cell validation
        List<Vector3> buildPosition = preview.BuildingModel.GetAllBuildingPosition();
        bool canBuild = true;

        // Validate each snapped unit's world position against the grid
        foreach (var pos in buildPosition)
        {
            BuildingGrid posGrid = BuildingGridManager.Instance.FindGridAtPosition(pos);
            if (posGrid == null || !posGrid.IsCellEmptyAtWorldPosition(pos))
            {
                canBuild = false;
                break;
            }
        }

        // 4. Handle states based on the snapped validation results
        if (canBuild)
        {
            preview.ChangeState(BuildingPreview.BuildingPreviewState.VALID);

            // Place building on left mouse click
            if (Input.GetMouseButtonDown(0) 
                && !Input.GetKey(KeyCode.Space) 
                && !Input.GetKey(KeyCode.R) 
                && !Input.GetKey(KeyCode.Q))
            {
                // If the EventSystem reports the pointer over UI, log the blocking UI elements
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    var ped = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
                    var results = new System.Collections.Generic.List<RaycastResult>();
                    EventSystem.current.RaycastAll(ped, results);
                    string names = results.Count == 0 ? "(no results)" : string.Join(", ", results.ConvertAll(r => r.gameObject.name));
                    Debug.Log($"Placement blocked by UI elements under pointer: {names}");
                    return;
                }

                // BATTLE SCENE PLACEMENT LOGIC
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
                    if (enableRemovingUnitFromArray)
                    {
                        var playerUnitsList = playerBattleSO.playerUnits.ToList();
                        var playerUnitsStatsList = playerBattleSO.playerUnitStats.ToList();
                        var selectedBuildingData = buildingDataList[buildingDataIndex];
                        playerUnitsList.Remove(selectedBuildingData);
                        if (selectedBuildingData != null && selectedBuildingData.unitPrefab != null)
                        {
                            playerUnitsStatsList.Remove(selectedBuildingData.unitPrefab);
                        }
                        playerBattleSO.playerUnits = playerUnitsList;
                        playerBattleSO.playerUnitStats = playerUnitsStatsList;
                        buildingDataList.Remove(preview.Data);
                        Object.FindFirstObjectByType<UnitButtonManager>()?.RefreshUnitButtons();
                    }
                }

                PlaceBuilding(buildPosition, primaryGrid);
                FinishPlacement();
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
        // Prefer a grid that contains the full set of positions to ensure cells
        // are assigned to the correct grid when placing across boundaries.
        // if ()
        // {
        //     //copy and paste the entire logic thats inside this Function
        // }
        BuildingGrid primaryGrid = BuildingGridManager.Instance.FindGridForPositions(buildPosition) ?? targetGrid ?? grid;

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

        // Transfer building data to the BuildingStatContainer
        BuildingStatContainer statContainer = building.GetComponentInChildren<BuildingStatContainer>();
        if (statContainer != null)
        {
            statContainer.buildingData = preview.Data;
        }

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

        decreaseResources(preview != null ? preview.Data : null);

        Destroy(preview.gameObject);
        preview = null;
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

    private void FinishPlacement()
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

    public void decreaseResources(BuildingData buildingData)
    {
        if (playerSO == null || buildingData == null)
        {
            return;
        }

        playerSO.coins -= buildingData.coinCost;
        playerSO.woodResources -= buildingData.woodCost;
        playerSO.stoneResources -= buildingData.rockCost;
        playerSO.farmResources -= buildingData.farmCost;
        playerSO.energyPoints -= buildingData.energyCost;
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