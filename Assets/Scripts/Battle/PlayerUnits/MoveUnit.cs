using System.Collections;
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
        turnManager = FindObjectOfType<TurnManager>();
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
        currentTurnPhase = turnManager.currentTurnPhase;
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
            if(rayHit)
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
        Debug.Log("Clicked on: " + obj.name);

        // If player clicked a highlighted tile, handle move/attack only if a player unit is selected
        var ht = obj.GetComponent<HighlightTile>();
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

            // If enemy present and tile is attackable, attempt attack via selected unit (only if it has attackActions)
            if (enemyPresent)
            {
                if (attacker != null && ht.isAttack && (selectedMove == null || selectedMove.attackActions > 0))
                {
                    bool attacked = attacker.TryAttackAtPosition(ht.worldPosition);
                    if (attacked) return; // attack occurred and highlights cleared
                }
                Debug.Log("Tile occupied by enemy — cannot move into it. Select your unit and click the highlighted tile to attack.");
                return;
            }

            // No enemy present — perform move only if the selected unit has remaining moveActions
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
                Debug.Log("Selected unit has no move actions remaining and cannot move, but highlights remain visible.");
            }

            return;
        }

        // If player clicked an enemy directly, attempt attack only if a player unit is selected
        if (obj != null && obj.CompareTag("EnemyUnit"))
        {
            var selected = CellHighlighter.Instance?.CurrentUnit;
            if (selected == null)
            {
                Debug.Log("No unit selected — select your unit first to attack an enemy.");
                return;
            }

            var attacker = selected.GetComponent<AttackEnemyUnit>();
            if (attacker == null)
            {
                Debug.Log("Selected unit cannot attack.");
                return;
            }

            int selAttackRange = 1;
            var selMove = selected.GetComponent<MoveUnit>();
            if (selMove != null) selAttackRange = selMove.attackRange;

            BuildingGrid[] grids = FindObjectsOfType<BuildingGrid>();
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
                (int ex, int ey) = grid.WorldToGridPosition(obj.transform.position);
                inRange = Mathf.Abs(sx - ex) <= selAttackRange && Mathf.Abs(sy - ey) <= selAttackRange;
            }
            else
            {
                float approxCell = 1f;
                if (grids != null && grids.Length > 0) approxCell = grids[0].CellSize;
                float maxDist = (selAttackRange + 0.5f) * approxCell;
                inRange = Vector3.Distance(selected.transform.position, obj.transform.position) <= maxDist;
            }

            if (inRange)
            {
                bool attacked = attacker.TryAttackAtPosition(obj.transform.position);
                if (attacked) return;
            }
            else
            {
                Debug.Log("Enemy is not within selected unit's attack range.");
            }

            return;
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

            // Select this unit and show its highlights
            clickedMove.isSelected = true;
            if (ch != null)
                ch.ShowHighlightsForUnit(obj, clickedMove.mobility, clickedMove.attackRange);
            return;
        }

        // Fallback: if the clicked object has a UnitSOContainer, you can read ranges from it
        UnitSOContainer container = obj.GetComponent<UnitSOContainer>();
        if (container != null && container.unitData != null)
        {
            // If your UnitSO has explicit fields for mobility/attack, map them here.
            // As a safe fallback, use this object's serialized values.
            if (CellHighlighter.Instance != null)
                CellHighlighter.Instance.ShowHighlightsForUnit(obj, mobility, attackRange);
            return;
        }

        // If nothing found, clear highlights
        if (CellHighlighter.Instance != null)
        {
            CellHighlighter.Instance.ClearHighlights();
        }
    }
    

    public void DetectObjects()
    {
        // If another ClickObject already handled this frame's click, skip
        if (lastProcessedClickFrame == Time.frameCount)
            return;

        // Mark this frame as handled so others skip
        lastProcessedClickFrame = Time.frameCount;

        Vector3 mousePos = Input.mousePosition;
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        rayHit = Physics.Raycast(ray, out hit);
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
    }

    public void MoveToPosition(Vector3 target)
    {
        // Preserve current Y so the assigned unitObject (or this) doesn't sink or float when moving on grid
        var moveTransform = unitObject != null ? unitObject.transform : transform;
        target.y = moveTransform.position.y;
        // Only consume a move action if available
        if (moveActions <= 0)
        {
            Debug.Log("No move actions available.");
            return;
        }
        moveActions = Mathf.Max(0, moveActions - 1);

        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveRoutine(target));
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