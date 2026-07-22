using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveUnit : MonoBehaviour
{
    public UnitSO unitData;

    private static int lastProcessedClickFrame = -1;
    private static int lastHandledClickFrame = -1;
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
    private RotateEnvironment rotateEnv;
    public turnPhase currentTurnPhase;
    public UnitStateMachine stateMachine;
    public bool isPlayerTurn;
    public bool isSelected;
    public bool isHidden;

    private readonly HashSet<TerrainHideUnit> activeFogTerrains = new HashSet<TerrainHideUnit>();

    private static int movingUnitCount = 0;
    public static bool AnyUnitMoving => movingUnitCount > 0;
    public bool IsMoving => isMoving;
    private bool isMoving;

    public GameObject unitObject;
    private RotateModel rotateModel;

    private void OnDisable()
    {
        SetMovingState(false);
    }

    private void OnDestroy()
    {
        SetMovingState(false);
    }

    private void SetMovingState(bool moving)
    {
        if (isMoving == moving)
            return;

        isMoving = moving;
        if (moving)
        {
            movingUnitCount++;
        }
        else
        {
            movingUnitCount = Mathf.Max(0, movingUnitCount - 1);
        }
    }

    public void EnterFog(TerrainHideUnit terrainHideUnit)
    {
        if (terrainHideUnit == null)
        {
            return;
        }

        activeFogTerrains.Add(terrainHideUnit);
        isHidden = activeFogTerrains.Count > 0;
    }

    public bool ExitFog(TerrainHideUnit terrainHideUnit)
    {
        if (terrainHideUnit == null)
        {
            return isHidden;
        }

        activeFogTerrains.Remove(terrainHideUnit);
        isHidden = activeFogTerrains.Count > 0;
        return isHidden;
    }

    void Awake()
    {
        UnitSOContainer container = this.GetComponent<UnitSOContainer>();
        turnManager = Object.FindAnyObjectByType<TurnManager>();
        rotateEnv = Object.FindAnyObjectByType<RotateEnvironment>();
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

        rotateModel = this.GetComponent<RotateModel>();
        if (rotateModel == null && unitObject != null)
        {
            rotateModel = unitObject.GetComponentInChildren<RotateModel>();
        }
        if (rotateModel == null)
        {
            rotateModel = this.GetComponentInChildren<RotateModel>();
        }
        if (rotateModel == null)
        {
            // Covers cases where the model/rotation script lives higher up the
            // hierarchy (e.g. on a grandparent "unit root" object) rather than
            // on this object, unitObject, or either of their children.
            rotateModel = this.GetComponentInParent<RotateModel>();
        }
        if (rotateModel == null)
        {
            Debug.LogWarning($"MoveUnit on '{gameObject.name}': No RotateModel found — model won't rotate when moving.");
        }
    }

    void Update()
    {
        if (rotateEnv != null && rotateEnv.IsRotating)
        {
            return;
        }
        currentTurnPhase = turnManager != null ? turnManager.currentTurnPhase : turnPhase.PlayerTurn;

        // Only allow selection input during PlayerTurn (not SetupTurn)
        if (currentTurnPhase == turnPhase.PlayerTurn)
        {
            isPlayerTurn = true;
        }
        else
        {
            isPlayerTurn = false;
        }

        DetectObjects();
        if (Input.GetMouseButtonDown(0) && isPlayerTurn && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) // Left mouse button
        {
            if (rayHit && hit.collider != null && lastHandledClickFrame != Time.frameCount)
            {
                lastHandledClickFrame = Time.frameCount;
                GameObject clickedObject = hit.collider.gameObject;
                Clicked(clickedObject);

                // NOTE: inspector-panel display used to also happen here, duplicating
                // the toggle-aware version in OnMouseDown and causing ShowUnitStats to
                // run twice per click. That's handled in OnMouseDown now.
            }
        }

        if (!isMoving)
        {
            // Don't stomp a one-shot Hurt/Damage reaction that's currently playing —
            // it will revert to Idle on its own once the clip finishes.
            if (stateMachine != null &&
                stateMachine.currentUnitPhase != unitPhase.Hurt &&
                stateMachine.currentUnitPhase != unitPhase.Damage)
            {
                stateMachine.ChangeState(unitPhase.Idle);
            }
        }
        else if(isMoving)
        {
            stateMachine?.ChangeState(unitPhase.Move);
        }
    }

//THIS IS THE ONE
    //CLICK LOGIC
    public void Clicked(GameObject obj)
    {
        // If player clicked a highlighted tile, handle move/attack/harvest only if a player unit is selected
        var ht = obj.GetComponent<HighlightTile>();
        if (ht != null)
        {
            var selected = CellHighlighter.Instance?.CurrentUnit;
            if (selected == null)
            {
                Debug.Log("No unit selected — select a player unit first before clicking tiles.");
                return;
            }

            float checkRadius = 0.4f;
            bool enemyPresent = false;
            Collider[] hits = Physics.OverlapSphere(ht.worldPosition, checkRadius);
            foreach (var h in hits)
            {
                if (h.CompareTag("EnemyUnit")) { enemyPresent = true; break; }

                var enemyHealth = h.GetComponentInParent<UnitHealth>();
                GameObject owner = enemyHealth != null ? enemyHealth.gameObject : null;
                if (owner == null)
                {
                    var enemyMove = h.GetComponentInParent<EnemyMovement>();
                    owner = enemyMove != null ? enemyMove.gameObject : null;
                }
                if (owner != null && (owner.CompareTag("EnemyUnit") || owner.GetComponentInParent<EnemyMovement>() != null))
                {
                    enemyPresent = true;
                    break;
                }
            }

            bool alliedPresent = false;
            foreach (var h in hits)
            {
                var allyMove = h.GetComponentInParent<MoveUnit>();
                if (allyMove == null) continue;

                // Don't block on the selected unit's own colliders.
                if (allyMove.gameObject == selected) continue;

                alliedPresent = true;
                break;
            }

            var selectedMove = selected.GetComponent<MoveUnit>();
            var attacker = selected.GetComponent<AttackEnemyUnit>();
            var harvester = selected.GetComponent<HarvestUnit>(); // Grab the harvester component

            // 1. This tile is within attack range AND something attackable (an enemy)
            // is actually standing on it — attack it. This applies whether the tile
            // is attack-only OR also within movement range (overlap tile): a target
            // being present always takes priority over walking onto its cell.
            if (ht.isAttack && enemyPresent)
            {
                if (attacker != null && (selectedMove == null || selectedMove.attackActions > 0))
                {
                    bool attacked = attacker.TryAttackAtPosition(ht.worldPosition);
                    if (attacked) return; // attack occurred and highlights cleared
                }
                Debug.Log("Tile occupied by enemy — cannot move into it. Select your unit and click the highlighted tile to attack.");
                return;
            }

            // 2. This tile is within attack range, has no enemy on it, but might have
            // harvestable terrain — try to harvest it. Also applies to overlap tiles.
            if (ht.isAttack && !enemyPresent && harvester != null)
            {
                if (selectedMove == null || selectedMove.attackActions > 0)
                {
                    // Execute harvest. This already consumes attackActions and clears highlights inside TryToHarvestPosition
                    bool harvested = harvester.TryToHarvestPosition(ht.worldPosition);
                    if (harvested) return; // there WAS harvestable terrain here
                }
                else
                {
                    Debug.Log("Selected unit has no attack actions left to harvest.");
                    return;
                }
                // Nothing was actually harvested (no terrain there) — fall through.
                // If this tile is also within movement range (overlap), it's just an
                // empty walkable tile and movement should still be allowed below.
            }

            // 3. Nothing to attack/harvest on this tile. Never allow walking onto a
            // tile that's occupied by an enemy which fell outside attack range
            // (i.e. it's inside the move diamond only, so step 1 above never fired).
            if (enemyPresent)
            {
                Debug.Log("Tile occupied by enemy and out of attack range — cannot move there.");
                return;
            }

            // 3b. Never allow walking onto a tile that's occupied by another
            // player unit — units cannot stack on the same cell.
            if (alliedPresent)
            {
                Debug.Log("Tile occupied by another of your units — cannot move there.");
                return;
            }

            // 4. Tile is genuinely empty (no enemy, no ally, nothing harvested).
            // Reject the move if it's only within attack range and not within the
            // unit's movement range (e.g. a ranged unit's attack square extends
            // further than its movement diamond). Overlap tiles pass this check
            // and are treated as ordinary walkable tiles.
            if (!ht.isMove)
            {
                Debug.Log($"Tile is beyond movement range (attack-only) — cannot move there. " +
                    $"[diag] unit={selected.name} mobility={(selectedMove != null ? selectedMove.mobility : -1)} " +
                    $"attackRange={(selectedMove != null ? selectedMove.attackRange : -1)} " +
                    $"moveActions={(selectedMove != null ? selectedMove.moveActions : -1)} " +
                    $"attackActions={(selectedMove != null ? selectedMove.attackActions : -1)} " +
                    $"tile={ht.worldPosition} ht.isAttack={ht.isAttack}");
                return;
            }

            // Perform move only if the selected unit has remaining moveActions
            if (selectedMove != null && selectedMove.canMove && selectedMove.moveActions > 0)
            {
                if (CellHighlighter.Instance != null)
                {
                    bool moved = CellHighlighter.Instance.MoveCurrentUnitTo(ht.worldPosition);
                    if (moved)
                    {
                        // Clear highlights after issuing move
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

        //===== ATTACKING ENEMIES DIRECTLY =====
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
                // Only allow target selection if we have attack actions left
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

        // Try to get a MoveUnit component from the clicked object (preferred)
        MoveUnit clickedMove = obj.GetComponent<MoveUnit>();
        if (clickedMove != null)
        {
            var ch = CellHighlighter.Instance;
            GameObject prevObj = ch != null ? ch.CurrentUnit : null;
            MoveUnit prevMove = prevObj != null ? prevObj.GetComponent<MoveUnit>() : null;

            // If clicking the same unit again -> deselect and clear highlights
            if (prevObj == obj)
            {
                clickedMove.isSelected = false;
                if (ch != null) ch.ClearHighlights();

                return;
            }

            // If another unit was selected, deselect it
            if (prevMove != null && prevMove != clickedMove)
            {
                prevMove.isSelected = false;
            }

            // Select this unit and show its highlights (only for player units)
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

        //  TERRAIN INTERACTION FALLBACK
        // If the click pierced through everything else and we are down to just the terrain layer, 
        // you can safely place your future custom terrain interaction logic here!
        var terrainComp = obj.GetComponentInParent<TerrainInteraction>();
        if (terrainComp != null)
        {
            Debug.Log($"Interacted directly with Terrain: {obj.name}. Future feature ready!");
            // Execute custom terrain actions here

            var terrainHarvest = obj.GetComponentInParent<TerrainHarvest>();
            if (terrainHarvest != null)
            {
                var selected = CellHighlighter.Instance?.CurrentUnit;
                if (selected == null)
                {
                    Debug.Log("No unit selected — select your unit first to attack an enemy.");
                    return;
                }

                var selectedMove = selected.GetComponent<MoveUnit>();
                // Only allow target selection if we have attack actions left
                if (selectedMove != null && selectedMove.attackActions <= 0)
                {
                    Debug.Log("Selected unit has no attack actions left.");
                    return;
                }

                var attacker = selected.GetComponent<HarvestUnit>();
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
                    bool attacked = attacker.TryToHarvestPosition(terrainHarvest.transform.position);
                    if (attacked) return;
                }
                else
                {
                    Debug.Log("Enemy is not within selected unit's attack range.");
                }
                return;
            }
        }

        // If nothing found, clear highlights
        if (CellHighlighter.Instance != null)
        {
            CellHighlighter.Instance.ClearHighlights();
        }
    }
    

    // FIXED DECTECT OBJECTS LAYER PIERCING LOGIC
// FIXED DETECT OBJECTS LAYER PIERCING LOGIC
    public void DetectObjects()
    {
        if (lastProcessedClickFrame == Time.frameCount)
            return;

        lastProcessedClickFrame = Time.frameCount;

        Vector3 mousePos = Input.mousePosition;
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        
        // Use RaycastAll to capture every single object underneath the cursor, sorted by distance
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        rayHit = false;
        hit = default;

        // --- STEP 1: PRIORITIZE HIGHLIGHT TILES (ACTION INPUTS) ---

        GameObject selectedUnit = CellHighlighter.Instance != null ? CellHighlighter.Instance.CurrentUnit : null;
        bool closestIsSelectedUnit = false;
        if (hits.Length > 0 && selectedUnit != null)
        {
            GameObject closestObj = hits[0].collider.gameObject;
            MoveUnit closestMove = closestObj.GetComponent<MoveUnit>() ?? closestObj.GetComponentInParent<MoveUnit>();
            if (closestMove != null && closestMove.gameObject == selectedUnit)
            {
                closestIsSelectedUnit = true;
            }
        }

        if (!closestIsSelectedUnit)
        {
            foreach (var h in hits)
            {
                GameObject g = h.collider.gameObject;
                if (g.GetComponent<HighlightTile>() != null)
                {
                    hit = h;
                    rayHit = true;
                    break;
                }
            }
        }

        // --- STEP 2: FALLBACK TO SELECTION (UNITS / ENEMIES) ---
        // If we didn't hit a HighlightTile, look for standard units to select or attack directly
        if (!rayHit)
        {
            foreach (var h in hits)
            {
                GameObject g = h.collider.gameObject;
                if (g.GetComponent<MoveUnit>() != null || 
                    g.GetComponentInParent<EnemyMovement>() != null ||
                    g.CompareTag("PlayerUnit") || 
                    g.CompareTag("EnemyUnit"))
                {
                    hit = h;
                    rayHit = true;
                    break;
                }
            }
        }

        // Step 3: If we didn't hit a gameplay system/unit, fall back to whatever was closest (like Terrain)
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

        if (grid != null)
        {
            // --- NEW: Check if the final target destination itself is blocked ---
            (int tx, int ty) = grid.WorldToGridPosition(target);
            Vector3 targetLocalCenter = new Vector3((tx + 0.5f) * grid.CellSize, 0.01f, (ty + 0.5f) * grid.CellSize);
            Vector3 targetWorldCenter = grid.transform.TransformPoint(targetLocalCenter);
            
            Collider[] targetCols = Physics.OverlapSphere(targetWorldCenter, grid.CellSize * 0.35f);
            foreach (var c in targetCols)
            {
                var ti = c.GetComponentInParent<TerrainInteraction>();
                if (ti != null && TerrainMatchesCell(grid, ti.transform.position, tx, ty) && ti.CantMoveOn())
                {
                    Debug.Log("Movement declined: Target cell is blocked by non-walkable terrain.");
                    return; // EXIT EARLY: Do not consume moveActions, do not move
                }
            }
            // ------------------------------------------------------------------

            // Your existing line-of-sight/path checking logic
            (int sx, int sy) = grid.WorldToGridPosition(moveTransform.position);
            (int ex, int ey) = grid.WorldToGridPosition(target);
            var path = GetCellsOnLine(sx, sy, ex, ey);
            for (int pi = 1; pi < path.Count; pi++)
            {
                var cell = path[pi];
                int x = cell.x; int y = cell.y;
                Vector3 localCenter = new Vector3((x + 0.5f) * grid.CellSize, 0.01f, (y + 0.5f) * grid.CellSize);
                Vector3 worldCenter = grid.transform.TransformPoint(localCenter);
                Collider[] cols = Physics.OverlapSphere(worldCenter, grid.CellSize * 0.35f);
                foreach (var c in cols)
                {
                    var ti = c.GetComponentInParent<TerrainInteraction>();
                    if (ti != null && TerrainMatchesCell(grid, ti.transform.position, x, y) && ti.CantWalkThrough())
                    {
                        target = worldCenter;
                        goto FoundBlockingTerrain;
                    }
                }
            }
        }

    FoundBlockingTerrain:
        // Double-check if the pathing adjustment brought us right back to where we started
        if (Vector3.Distance(moveTransform.position, target) < 0.1f)
        {
            Debug.Log("Movement declined: Blocked by terrain immediately ahead.");
            return; // EXIT EARLY
        }

        target.y = moveTransform.position.y;
        if (moveActions <= 0)
        {
            Debug.Log("No move actions available.");
            return;
        }
        
        // Safe to consume action and move
        moveActions = Mathf.Max(0, moveActions - 1);

        // Turn the model to face the direction it's about to travel in.
        rotateModel?.FaceDirection(target - moveTransform.position);

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
            SetMovingState(false);
        }

        moveCoroutine = StartCoroutine(MoveRoutine(target));
        SetMovingState(true);
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
        SetMovingState(false);
    }

    private bool TerrainMatchesCell(BuildingGrid grid, Vector3 terrainWorldPosition, int gx, int gy)
    {
        if (grid == null) return false;

        (int terrainX, int terrainY) = grid.WorldToGridPosition(terrainWorldPosition);
        return terrainX == gx && terrainY == gy;
    }

    private void OnMouseDown()
    {
        // 1. Maintain complete protection against clicking through UI buttons
        if (UnityEngine.EventSystems.EventSystem.current != null && 
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) 
            return;

        Debug.Log($"[INPUT REGISTERED] Unit clicked: {gameObject.name}");

        UnitHealth healthComp = GetComponent<UnitHealth>();
        if (healthComp != null && healthComp.unitData != null)
        {
            if (MinimizedInspector.Instance != null)
            {
                if (MinimizedInspector.Instance.gameObject.activeSelf && 
                    MinimizedInspector.Instance.nameText != null && 
                    MinimizedInspector.Instance.nameText.text == healthComp.unitData.unitName)
                {
                    Debug.Log($"[TOGGLE] '{healthComp.unitData.unitName}' clicked again. Closing inspector panel...");
                    MinimizedInspector.Instance.CloseInspector(); // Cleanly turns it off[cite: 7]
                }
                else
                {
                    Debug.Log($"[TOGGLE] Opening inspector panel for '{healthComp.unitData.unitName}'...[cite: 7]");
                    // Otherwise, show/update it normally[cite: 7]
                    MinimizedInspector.Instance.ShowUnitStats(
                        healthComp.unitData, 
                        healthComp.currentHealth, 
                        healthComp.maxHealth
                    );
                }
            }
        }
    }
}