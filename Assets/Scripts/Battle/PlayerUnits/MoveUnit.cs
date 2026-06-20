using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveUnit : MonoBehaviour
{
    public UnitSO unitData;

    private static int lastProcessedClickFrame = -1;
    private Camera mainCamera;

    private Material originalMaterial;
    bool rayHit;
    RaycastHit hit;

    [Header("Range (set per-unit or via UnitSO)")]
    public int mobility = 2; // Manhattan (diamond/cross) movement range
    public int attackRange = 1; // Square attack range


    [Header("Movement")]
    public float moveSpeed = 4f;
    public int moveActions = 1;
    public int attackActions = 1;
    public bool canMove = true;

    [Header("States")]
    private Coroutine moveCoroutine;
    private TurnManager turnManager;
    public turnPhase currentTurnPhase;
    public UnitStateMachine stateMachine;
    public bool isPlayerTurn;
    public bool isSelected;

    public GameObject unitObject;

    void Awake()
    {
        UnitSOContainer container = this.GetComponent<UnitSOContainer>();
        turnManager = Object.FindAnyObjectByType<TurnManager>();
        if (container != null)
        {
            unitData = container.unitData;
        }
        if (mainCamera == null)
            mainCamera = Camera.main;

        mobility = unitData != null ? unitData.mobility : mobility;
        attackRange = unitData != null ? unitData.attackRange : attackRange;
        attackActions = unitData != null ? unitData.attackPoints : attackActions;
        moveActions = unitData != null ? unitData.movePoints : moveActions;
        stateMachine = this.GetComponent<UnitStateMachine>();
    }

    void Update()
    {
        currentTurnPhase = turnManager != null ? turnManager.currentTurnPhase : turnPhase.PlayerTurn;
        if (currentTurnPhase == turnPhase.PlayerTurn)
        {
            isPlayerTurn = true;
        }
        else
        {
            isPlayerTurn = false;
        }

        DetectObjects();
        if (Input.GetMouseButtonDown(0) && isPlayerTurn) // Left mouse button
        {
            if (rayHit && hit.collider != null)
            {
                Clicked(hit.collider.gameObject);
            }
        }

        if (isSelected == true)
        {
            stateMachine.currentUnitPhase = unitPhase.Selected;
        }
        else if (isSelected == false)
        {
            stateMachine.currentUnitPhase = unitPhase.Idle;
        }
    }

    //CLICK LOGIC
    public void Clicked(GameObject obj)
    {
        // FIXED: Use GetComponentInParent to safely find the component if a child collider was clicked
        var ht = obj.GetComponentInParent<HighlightTile>();
        if (ht != null)
        {
            var selected = CellHighlighter.Instance?.CurrentUnit;
            if (selected == null)
            {
                Debug.Log("No unit selected — select a player unit first before clicking tiles.");
                return;
            }

            // Detect if an enemy occupies the tile
            float checkRadius = 0.4f;
            bool enemyPresent = false;
            Collider[] hits = Physics.OverlapSphere(ht.worldPosition, checkRadius);
            foreach (var h in hits)
            {
                if (h.CompareTag("EnemyUnit")) { enemyPresent = true; break; }
            }

            var selectedMove = selected.GetComponent<MoveUnit>();
            var attacker = selected.GetComponent<AttackEnemyUnit>();

            // Prioritize ranged attack over movement if the tile is an attack tile 
            if (enemyPresent && ht.isAttack)
            {
                if (attacker != null && (selectedMove == null || selectedMove.attackActions > 0))
                {
                    bool attacked = attacker.TryAttackAtPosition(ht.worldPosition);
                    if (attacked) return; 
                }
                Debug.Log("Tile occupied by enemy — cannot move into it. Select your unit and click the highlighted tile to attack.");
                return;
            }

            // No enemy present or not an attack tile — perform move only if the selected unit has remaining moveActions
            if (selectedMove != null && selectedMove.canMove && selectedMove.moveActions > 0)
            {
                if (CellHighlighter.Instance != null)
                {
                    bool moved = CellHighlighter.Instance.MoveCurrentUnitTo(ht.worldPosition);
                    if (moved)
                    {
                        CellHighlighter.Instance.ClearHighlights();
                    }
                }
            }
            else
            {
                Debug.Log("Selected unit has no move actions remaining and cannot move.");
            }

            return;
        }

        // If player clicked an enemy directly (or a child collider), attempt attack only if a player unit is selected
        if (obj != null)
        {
            var enemyComp = obj.GetComponentInParent<EnemyMovement>();
            if (enemyComp != null)
            {
                var enemyRoot = enemyComp.gameObject;
                var selected = CellHighlighter.Instance?.CurrentUnit;
                if (selected == null)
                {
                    Debug.Log("No unit selected — select your unit first to attack an enemy.");
                    return;
                }

                var selectedMove = selected.GetComponent<MoveUnit>();
                if (selectedMove != null && selectedMove.attackActions <= 0)
                {
                    Debug.Log("Selected unit has no attack actions left.");
                    return;
                }

                var attacker = selected.GetComponent<AttackEnemyUnit>();
                if (attacker == null)
                {
                    Debug.Log("Selected unit cannot attack.");
                    return;
                }

                int selAttackRange = 1;
                if (selectedMove != null) selAttackRange = selectedMove.attackRange;

                BuildingGrid[] grids = UnityEngine.Object.FindObjectsByType<BuildingGrid>(UnityEngine.FindObjectsSortMode.None);
                BuildingGrid grid = null;
                foreach (var g in grids)
                {
                    if (g.ContainsWorldPosition(selected.transform.position))
                    {
                        grid = g;
                        break;
                    }
                }

                bool inRange = false;
                if (grid != null)
                {
                    (int sx, int sy) = grid.WorldToGridPosition(selected.transform.position);
                    (int ex, int ey) = grid.WorldToGridPosition(enemyRoot.transform.position);
                    inRange = Mathf.Abs(sx - ex) <= selAttackRange && Mathf.Abs(sy - ey) <= selAttackRange;
                }
                else
                {
                    float approxCell = 1f;
                    if (grids != null && grids.Length > 0) approxCell = grids[0].CellSize;
                    float maxDist = (selAttackRange + 0.5f) * approxCell;
                    inRange = Vector3.Distance(selected.transform.position, enemyRoot.transform.position) <= maxDist;
                }

                if (inRange)
                {
                    bool attacked = attacker.TryAttackAtPosition(enemyRoot.transform.position);
                    if (attacked) return;
                }
                else
                {
                    Debug.Log("Enemy is not within selected unit's attack range.");
                }

                return;
            }
        }

        // --- HARVEST TERRAIN INTERACTION ---
        var terrainHarvest = obj.GetComponentInParent<TerrainHarvest>();
        if (terrainHarvest != null)
        {
            var selected = CellHighlighter.Instance?.CurrentUnit;
            if (selected != null)
            {
                var harvester = selected.GetComponent<HarvestResource>();
                var selectedMove = selected.GetComponent<MoveUnit>();

                if (harvester != null && selectedMove != null)
                {
                    if (terrainHarvest.hasHarvested)
                    {
                        Debug.Log("This terrain node has already been harvested!");
                        return;
                    }

                    if (selectedMove.attackActions <= 0)
                    {
                        Debug.Log("Unit has no attack actions left to harvest.");
                        return;
                    }

                    int selAttackRange = selectedMove.attackRange;
                    BuildingGrid[] grids = UnityEngine.Object.FindObjectsByType<BuildingGrid>(UnityEngine.FindObjectsSortMode.None);
                    BuildingGrid grid = null;
                    foreach (var g in grids)
                    {
                        if (g.ContainsWorldPosition(selected.transform.position))
                        {
                            grid = g;
                            break;
                        }
                    }

                    bool inRange = false;
                    if (grid != null)
                    {
                        (int sx, int sy) = grid.WorldToGridPosition(selected.transform.position);
                        (int ex, int ey) = grid.WorldToGridPosition(terrainHarvest.transform.position);
                        inRange = Mathf.Abs(sx - ex) <= selAttackRange && Mathf.Abs(sy - ey) <= selAttackRange;
                    }
                    else
                    {
                        float approxCell = 1f;
                        if (grids != null && grids.Length > 0) approxCell = grids[0].CellSize;
                        float maxDist = (selAttackRange + 0.5f) * approxCell;
                        inRange = Vector3.Distance(selected.transform.position, terrainHarvest.transform.position) <= maxDist;
                    }

                    if (inRange)
                    {
                        terrainHarvest.Harvest(harvester.harvestAmount, selectedMove);
                        CellHighlighter.Instance?.ClearHighlights();
                        return;
                    }
                    else
                    {
                        Debug.Log("Terrain node is outside of the unit's attack range.");
                    }
                }
            }
        }

        // Try to get a MoveUnit component from the clicked object (preferred)
        // FIXED: Using GetComponentInParent here to capture selection smoothly
        MoveUnit clickedMove = obj.GetComponentInParent<MoveUnit>();
        if (clickedMove != null)
        {
            var ch = CellHighlighter.Instance;
            GameObject prevObj = ch != null ? ch.CurrentUnit : null;
            MoveUnit prevMove = prevObj != null ? prevObj.GetComponent<MoveUnit>() : null;

            if (prevObj == clickedMove.gameObject)
            {
                clickedMove.isSelected = false;
                if (ch != null) ch.ClearHighlights();
                return;
            }

            if (prevMove != null && prevMove != clickedMove)
            {
                prevMove.isSelected = false;
            }

            clickedMove.isSelected = true;
            if (ch != null)
            {
                GameObject unitRoot = clickedMove.gameObject;
                if (unitRoot.CompareTag("PlayerUnit") || unitRoot.GetComponentInParent<MoveUnit>() != null)
                {
                    if (clickedMove.moveActions <= 0 && clickedMove.attackActions <= 0)
                    {
                        ch.ClearHighlights();
                        Debug.Log("Unit selected, but has no actions remaining. Grid will not highlight.");
                    }
                    else
                    {
                        int structuralMobility = clickedMove.moveActions > 0 ? clickedMove.mobility : 0;
                        int structuralAttackRange = clickedMove.attackActions > 0 ? clickedMove.attackRange : 0;
                        
                        ch.ShowHighlightsForUnit(unitRoot, structuralMobility, structuralAttackRange);
                    }
                }
            }
            return;
        }

        // Fallback: if the clicked object has a UnitSOContainer
        UnitSOContainer container = obj.GetComponentInParent<UnitSOContainer>();
        if (container != null && container.unitData != null)
        {
            GameObject unitRoot = container.gameObject;
            bool isPlayer = unitRoot.CompareTag("PlayerUnit") || obj.CompareTag("PlayerUnit") || obj.GetComponentInParent<MoveUnit>() != null;
            if (isPlayer)
            {
                if (moveActions > 0 || attackActions > 0)
                {
                    if (CellHighlighter.Instance != null)
                    {
                        int structuralMobility = moveActions > 0 ? mobility : 0;
                        int structuralAttackRange = attackActions > 0 ? attackRange : 0;
                        CellHighlighter.Instance.ShowHighlightsForUnit(unitRoot, structuralMobility, structuralAttackRange);
                    }
                }
                else
                {
                    CellHighlighter.Instance?.ClearHighlights();
                }
            }
            return;
        }

        // --- TERRAIN INTERACTION FALLBACK ---
        var terrainComp = obj.GetComponentInParent<TerrainInteraction>();
        if (terrainComp != null)
        {
            return;
        }

        if (CellHighlighter.Instance != null)
        {
            CellHighlighter.Instance.ClearHighlights();
        }
    }

    // DECTECT OBJECTS LAYER PIERCING LOGIC
    public void DetectObjects()
    {
        if (lastProcessedClickFrame == Time.frameCount)
            return;

        lastProcessedClickFrame = Time.frameCount;

        Vector3 mousePos = Input.mousePosition;
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        rayHit = false;
        hit = default;

        // FIXED: Look through all objects using GetComponentInParent to pierce blocking meshes cleanly
        foreach (var h in hits)
        {
            GameObject g = h.collider.gameObject;
            if (g.GetComponentInParent<HighlightTile>() != null || 
                g.GetComponentInParent<MoveUnit>() != null || 
                g.GetComponentInParent<TerrainHarvest>() != null ||
                g.GetComponentInParent<EnemyMovement>() != null ||
                g.CompareTag("PlayerUnit") || 
                g.CompareTag("EnemyUnit"))
            {
                hit = h;
                rayHit = true;
                break;
            }
        }

        // Step 2: Fallback to whatever was closest (like decorative Terrain meshes) if no prioritized UI/gameplay system was hit
        if (!rayHit && hits.Length > 0)
        {
            hit = hits[0];
            rayHit = true;
        }

        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
    }

    public void MoveToPosition(Vector3 target)
    {
        var moveTransform = unitObject != null ? unitObject.transform : transform;
        BuildingGrid[] grids = UnityEngine.Object.FindObjectsByType<BuildingGrid>(UnityEngine.FindObjectsSortMode.None);
        BuildingGrid grid = null;
        if (grids != null && grids.Length > 0)
        {
            foreach (var g in grids)
            {
                if (g.ContainsWorldPosition(moveTransform.position))
                {
                    grid = g;
                    break;
                }
            }
            if (grid == null) grid = grids[0];
        }

        Vector3 finalTarget = target;

        if (grid != null)
        {
            (int sx, int sy) = grid.WorldToGridPosition(moveTransform.position);
            (int ex, int ey) = grid.WorldToGridPosition(target);
            var path = GetCellsOnLine(sx, sy, ex, ey);
            
            int finalIndex = path.Count - 1;

            // Step 1: Scan path forward for absolute structural blockades (e.g., walls)
            for (int pi = 1; pi < path.Count; pi++)
            {
                var cell = path[pi];
                Vector3 worldCenter = grid.transform.TransformPoint(new Vector3((cell.x + 0.5f) * grid.CellSize, 0.01f, (cell.y + 0.5f) * grid.CellSize));
                Collider[] cols = Physics.OverlapSphere(worldCenter, grid.CellSize * 0.35f);
                foreach (var c in cols)
                {
                    var ti = c.GetComponentInParent<TerrainInteraction>();
                    if (ti != null && ti.CantWalkThrough())
                    {
                        finalIndex = pi - 1;
                        break;
                    }
                }
                if (finalIndex < path.Count - 1) break;
            }

            // Step 2: Step backward along the calculated path if the landing spot has !canMoveOn
            while (finalIndex > 0)
            {
                var cell = path[finalIndex];
                Vector3 worldCenter = grid.transform.TransformPoint(new Vector3((cell.x + 0.5f) * grid.CellSize, 0.01f, (cell.y + 0.5f) * grid.CellSize));
                Collider[] cols = Physics.OverlapSphere(worldCenter, grid.CellSize * 0.35f);
                
                bool landingIsBlocked = false;
                foreach (var c in cols)
                {
                    var ti = c.GetComponentInParent<TerrainInteraction>();
                    if (ti != null && !ti.canMoveOn)
                    {
                        landingIsBlocked = true;
                        break;
                    }
                }

                if (landingIsBlocked)
                {
                    finalIndex--; // Fall back to the adjacent neighbor along the path sequence
                }
                else
                {
                    break;
                }
            }

            var finalCell = path[finalIndex];
            finalTarget = grid.transform.TransformPoint(new Vector3((finalCell.x + 0.5f) * grid.CellSize, 0.01f, (finalCell.y + 0.5f) * grid.CellSize));
        }

        finalTarget.y = moveTransform.position.y;

        // If completely blocked from making progressive steps, exit cleanly preserving actions
        float approxSameCellRadius = grid != null ? grid.CellSize * 0.4f : 0.1f;
        if (Vector3.Distance(moveTransform.position, finalTarget) <= approxSameCellRadius)
        {
            Debug.Log("Movement invalid or destination completely blocked. Move action preserved.");
            return;
        }

        if (moveActions <= 0)
        {
            Debug.Log("No move actions available.");
            return;
        }
        
        // Deduct action point
        moveActions = Mathf.Max(0, moveActions - 1);

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveRoutine(finalTarget));
    }

    private List<Vector2Int> GetCellsOnLine(int x0, int y0, int x1, int y1)
    {
        var cells = new List<Vector2Int>();
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        int x = x0;
        int y = y0;
        while (true)
        {
            cells.Add(new Vector2Int(x, y));
            if (x == x1 && y == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }
        }
        return cells;
    }

    IEnumerator MoveRoutine(Vector3 target)
    {
        float stopSq = 0.001f;
        var moveTransform = unitObject != null ? unitObject.transform : transform;
        while ((moveTransform.position - target).sqrMagnitude > stopSq)
        {
            moveTransform.position = Vector3.MoveTowards(moveTransform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        moveTransform.position = target;
        moveCoroutine = null;
    }
}