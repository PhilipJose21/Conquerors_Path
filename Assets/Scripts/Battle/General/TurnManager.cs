using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public enum turnPhase
{
    PlayerTurn,
    EnemyTurn,
    SetupTurn,
    StartPlayerTurn,
    StartEnemyTurn,
    PlayerWin,
    EnemyWin
}

public class TurnManager : MonoBehaviour
{
    public float transitionTime = 1f;
    public bool placementPhase;
    public BuildingSystem buildingSystem;
    public turnPhase currentTurnPhase;
    public TextMeshProUGUI turnPhaseText;
    public GameObject[] playerUnits;
    public GameObject[] enemyUnits;
    public float enemyTurnDelay = 1f;

    public GameObject gameOverScreen;
    public GameObject victoryScreen;
    public GameObject playerTurnScreen;
    public GameObject enemyTurnScreen;

    private Coroutine transitionCoroutine;
    private bool transitionPending = false;
    private bool isEnemyTurnProcessing = false;
    
    void Awake()
    {
        buildingSystem = Object.FindFirstObjectByType<BuildingSystem>();
    }

    void Start()
    {
        currentTurnPhase = turnPhase.SetupTurn;
    }

    void Update()
    {
        // 1. Check Win/Loss conditions FIRST before handling any phase logic
        if (currentTurnPhase != turnPhase.SetupTurn && currentTurnPhase != turnPhase.PlayerWin && currentTurnPhase != turnPhase.EnemyWin)
        {
            
            updateUnitLists();
            if (playerUnits == null || playerUnits.Length == 0)
            {
                currentTurnPhase = turnPhase.EnemyWin;
                gameOverScreen.SetActive(true);
                HideTurnScreens(); // Clean up UI instantly
                return; 
            }
            else if (enemyUnits == null || enemyUnits.Length == 0)
            {
                currentTurnPhase = turnPhase.PlayerWin;
                victoryScreen.SetActive(true);
                HideTurnScreens(); // Clean up UI instantly
                return;
            }
        }

        // 2. Only run turn logic if the game is still actively going
        if (currentTurnPhase != turnPhase.PlayerWin && currentTurnPhase != turnPhase.EnemyWin)
        {
            checkTurnPhase();
        }
    }

    // Helper method to make sure turn banners are stripped away on Win/Loss
    private void HideTurnScreens()
    {
        if (playerTurnScreen != null) playerTurnScreen.SetActive(false);
        if (enemyTurnScreen != null) enemyTurnScreen.SetActive(false);
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionPending = false;
        }
    }

    public void checkTurnPhase()
    {
        switch (currentTurnPhase)
        {
            case turnPhase.SetupTurn:
                if (buildingSystem != null)
                    buildingSystem.gameObject.SetActive(true);
                    buildingSystem.enableReinforcementCost = false;

                if (placementPhase == false)
                {
                    currentTurnPhase = turnPhase.StartPlayerTurn;
                    if (buildingSystem != null) buildingSystem.gameObject.SetActive(false);
                }
                break;

            case turnPhase.StartPlayerTurn:
                foreach (var unit in playerUnits)
                {
                    if (unit == null) continue;
                    var moveUnit = unit.GetComponentInChildren<MoveUnit>();
                    if (moveUnit != null)
                    {
                        buildingSystem.gameObject.SetActive(true);
                        buildingSystem.enableReinforcementCost = true;
                        moveUnit.moveActions = moveUnit.unitData != null ? moveUnit.unitData.movePoints : moveUnit.moveActions;
                        moveUnit.attackActions = moveUnit.unitData != null ? moveUnit.unitData.attackPoints : moveUnit.attackActions;
                    }
                }
                if (!transitionPending)
                    TransitionToPhase(turnPhase.PlayerTurn);
                break;
            
            case turnPhase.PlayerTurn:
                bool anyPlayerCanAct = false;
                foreach (var unit in playerUnits)
                {
                    if (unit == null) continue;
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
                    if (CellHighlighter.Instance != null) CellHighlighter.Instance.ClearHighlights();
                    currentTurnPhase = turnPhase.StartEnemyTurn;
                }
                break;

            case turnPhase.StartEnemyTurn:
                foreach (var unit in enemyUnits)
                {
                    if (unit == null) continue;
                    var moveUnit = unit.GetComponentInChildren<EnemyMovement>();
                    if (moveUnit != null)
                    {
                        moveUnit.moveActions = moveUnit.unitData != null ? moveUnit.unitData.movePoints : moveUnit.moveActions;
                        moveUnit.attackActions = moveUnit.unitData != null ? moveUnit.unitData.attackPoints : moveUnit.attackActions;
                        moveUnit.endTurn = false;
                    }
                }
                if (!transitionPending)
                    TransitionToPhase(turnPhase.EnemyTurn);
                break;
            
            case turnPhase.EnemyTurn:
                if (!isEnemyTurnProcessing)
                {
                    isEnemyTurnProcessing = true;
                    StartCoroutine(EnemyTurnSequence());
                }
                break;
        }
            
        if (turnPhaseText != null) turnPhaseText.text = currentTurnPhase.ToString();
    }

    public void EndPlayerTurn()
    {
        if (currentTurnPhase == turnPhase.PlayerTurn)
        {
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
                if (enemyUnits[i] == null) continue;
                var moveUnit = enemyUnits[i].GetComponent<EnemyMovement>();
                if (moveUnit != null)
                {
                    moveUnit.endTurn = true;
                }
            }
            currentTurnPhase = turnPhase.StartPlayerTurn;
        }
    }

    public void TransitionToPhase(turnPhase newPhase)
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);
        transitionPending = true;
        transitionCoroutine = StartCoroutine(TransitionCoroutine(newPhase));
    }

    private System.Collections.IEnumerator TransitionCoroutine(turnPhase newPhase)
    {
        // Safety Check: If a win/loss occurred right as this triggered, abort.
        if (currentTurnPhase == turnPhase.PlayerWin || currentTurnPhase == turnPhase.EnemyWin)
        {
            HideTurnScreens();
            yield break;
        }

        if (newPhase == turnPhase.PlayerTurn)
        {
            playerTurnScreen.SetActive(true);
            enemyTurnScreen.SetActive(false);
        }
        else if (newPhase == turnPhase.EnemyTurn)
        {
            playerTurnScreen.SetActive(false);
            enemyTurnScreen.SetActive(true);
        }

        yield return new WaitForSeconds(transitionTime);

        // Safety Check: Did a unit die via a status effect/hazard during the wait time?
        if (currentTurnPhase == turnPhase.PlayerWin || currentTurnPhase == turnPhase.EnemyWin)
        {
            HideTurnScreens();
            yield break;
        }

        playerTurnScreen.SetActive(false);
        enemyTurnScreen.SetActive(false);
        currentTurnPhase = newPhase;
        transitionCoroutine = null;
        transitionPending = false;
    }

    private System.Collections.IEnumerator EnemyTurnSequence()
    {
        for (int i = 0; i < enemyUnits.Length; i++)
        {
            // Abort processing the sequence if the player won mid-enemy turn (e.g., counter-attack)
            if (currentTurnPhase == turnPhase.PlayerWin || currentTurnPhase == turnPhase.EnemyWin)
            {
                isEnemyTurnProcessing = false;
                yield break;
            }

            if (enemyUnits[i] != null)
            {
                var moveUnit = enemyUnits[i].GetComponentInChildren<EnemyMovement>();
                if (moveUnit != null && !moveUnit.endTurn)
                {
                    moveUnit.Act();
                    yield return new WaitForSeconds(enemyTurnDelay);
                }
            }
        }
        
        if (currentTurnPhase != turnPhase.PlayerWin && currentTurnPhase != turnPhase.EnemyWin)
        {
            currentTurnPhase = turnPhase.StartPlayerTurn;
        }
        isEnemyTurnProcessing = false;
    }

    public void endSetup()
    {
        updateUnitLists();
        if (playerUnits.Length > 0)
        {
            placementPhase = false;
        }
    }

    public void updateUnitLists()
    {
        playerUnits = GameObject.FindGameObjectsWithTag("PlayerUnit");
        enemyUnits = GameObject.FindGameObjectsWithTag("EnemyUnit");
    }
}