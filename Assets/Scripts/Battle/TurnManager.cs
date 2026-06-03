using UnityEngine;
using TMPro;

public enum turnPhase
    {
        PlayerTurn,
        EnemyTurn,
        SetupTurn,
        StartPlayerTurn,
        StartEnemyTurn
    }

public class TurnManager : MonoBehaviour
{

    

    public turnPhase currentTurnPhase;
    public TextMeshProUGUI turnPhaseText;

    public GameObject[] playerUnits;
    public GameObject[] enemyUnits;

    public float transitionTime = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    void Awake()
    {
        playerUnits = GameObject.FindGameObjectsWithTag("PlayerUnit");
        enemyUnits = GameObject.FindGameObjectsWithTag("EnemyUnit");
    }

    void Start()
    {
        currentTurnPhase = turnPhase.PlayerTurn;
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentTurnPhase)
        {
            case turnPhase.StartPlayerTurn:
                // Refresh player list in case units were removed/added during the enemy turn
                playerUnits = GameObject.FindGameObjectsWithTag("PlayerUnit");
                foreach (var unit in playerUnits)
                {
                    if (unit == null) continue;
                    // Use GetComponentInChildren in case MoveUnit is on a child object
                    var moveUnit = unit.GetComponentInChildren<MoveUnit>();
                    if (moveUnit != null)
                    {
                        // If unitData is available use its values, otherwise keep current values
                        moveUnit.moveActions = moveUnit.unitData != null ? moveUnit.unitData.movePoints : moveUnit.moveActions;
                        moveUnit.attackActions = moveUnit.unitData != null ? moveUnit.unitData.attackPoints : moveUnit.attackActions;
                    }
                }
                // After initializing player units, transition to PlayerTurn (schedule once)
                if (!transitionPending)
                    TransitionToPhase(turnPhase.PlayerTurn);
                break;
            
            case turnPhase.PlayerTurn:
                bool anyPlayerCanAct = false;
                foreach (var unit in playerUnits)
                {
                    var moveUnit = unit.GetComponentInChildren<MoveUnit>();
                    if (moveUnit != null)
                    {
                        if (moveUnit.moveActions > 0 || moveUnit.attackActions > 0)
                        {
                            anyPlayerCanAct = true;
                            break;
                        }
                    }
                }

                if (!anyPlayerCanAct)
                {
                    // Clear any move/attack highlights when the player turn ends automatically
                    if (CellHighlighter.Instance != null) CellHighlighter.Instance.ClearHighlights();
                    currentTurnPhase = turnPhase.StartEnemyTurn;
                }
                else
                {
                    // At least one player unit can act, remain in PlayerTurn
                    return;
                }
                break;

            case turnPhase.StartEnemyTurn:
                // Refresh enemy list (in case enemies were added/removed) and reset their action points
                enemyUnits = GameObject.FindGameObjectsWithTag("EnemyUnit");
                foreach (var unit in enemyUnits)
                {
                    // Use GetComponentInChildren to support EnemyMovement on child objects
                    var moveUnit = unit.GetComponentInChildren<EnemyMovement>();
                    if (moveUnit != null)
                    {
                        moveUnit.moveActions = moveUnit.unitData != null ? moveUnit.unitData.movePoints : moveUnit.moveActions;
                        moveUnit.attackActions = moveUnit.unitData != null ? moveUnit.unitData.attackPoints : moveUnit.attackActions;
                        // Reset per-unit endTurn flag when beginning the enemy turn
                        moveUnit.endTurn = false;
                    }
                }
                // After initializing enemy units, transition to EnemyTurn (schedule once)
                if (!transitionPending)
                    TransitionToPhase(turnPhase.EnemyTurn);
                break;
            
            case turnPhase.EnemyTurn:
                // Advance to StartPlayerTurn only when ALL enemy units have finished (no actions or flagged endTurn)
                bool anyEnemyCanAct = false;
                foreach (var unit in enemyUnits)
                {
                    var moveUnit = unit.GetComponent<EnemyMovement>();
                    if (moveUnit != null)
                    {
                        bool hasActions = moveUnit.moveActions > 0 || moveUnit.attackActions > 0;
                        if (hasActions && !moveUnit.endTurn)
                        {
                            anyEnemyCanAct = true;
                            break;
                        }
                    }
                }

                if (!anyEnemyCanAct)
                {
                    currentTurnPhase = turnPhase.StartPlayerTurn;
                }
                break;
        }
            
        turnPhaseText.text = currentTurnPhase.ToString();
    }

    public void EndPlayerTurn()
    {
        if (currentTurnPhase == turnPhase.PlayerTurn)
        {
            // Clear highlights to tidy the battlefield when the player ends their turn
            if (CellHighlighter.Instance != null) CellHighlighter.Instance.ClearHighlights();
            currentTurnPhase = turnPhase.StartEnemyTurn;
        }
    }

    public void EndEnemyTurn()
    {
        if (currentTurnPhase == turnPhase.EnemyTurn || currentTurnPhase == turnPhase.StartEnemyTurn)
        {
            for (int i = 0; i < enemyUnits.Length; i++)
            {
                var moveUnit = enemyUnits[i].GetComponent<EnemyMovement>();
                if (moveUnit != null)
                {
                    moveUnit.endTurn = true;
                }
                currentTurnPhase = turnPhase.StartPlayerTurn;
            }
        }
    }

    public void TransitionToPhase(turnPhase newPhase)
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);
        transitionPending = true;
        transitionCoroutine = StartCoroutine(TransitionCoroutine(newPhase));
    }

    private Coroutine transitionCoroutine;
    private System.Collections.IEnumerator TransitionCoroutine(turnPhase newPhase)
    {
        yield return new WaitForSeconds(transitionTime);
        currentTurnPhase = newPhase;
        transitionCoroutine = null;
        transitionPending = false;
    }

    // Prevent scheduling multiple concurrent transitions
    private bool transitionPending = false;
}
