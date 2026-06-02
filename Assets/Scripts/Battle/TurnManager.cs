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
                foreach (var unit in playerUnits)
                {
                    var moveUnit = unit.GetComponent<MoveUnit>();
                    if (moveUnit != null)
                    {
                        moveUnit.moveActions = moveUnit.unitData != null ? moveUnit.unitData.movePoints : moveUnit.moveActions;
                        moveUnit.attackActions = moveUnit.unitData != null ? moveUnit.unitData.attackPoints : moveUnit.attackActions;
                    }
                }

                currentTurnPhase = turnPhase.PlayerTurn;
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
                    currentTurnPhase = turnPhase.StartEnemyTurn;
                }
                else
                {
                    // At least one player unit can act, remain in PlayerTurn
                    return;
                }
                break;

            case turnPhase.StartEnemyTurn:
                foreach (var unit in enemyUnits)
                {
                    var moveUnit = unit.GetComponent<MoveUnit>();
                    if (moveUnit != null)
                    {
                        moveUnit.moveActions = moveUnit.unitData != null ? moveUnit.unitData.movePoints : moveUnit.moveActions;
                        moveUnit.attackActions = moveUnit.unitData != null ? moveUnit.unitData.attackPoints : moveUnit.attackActions;
                    }
                }
                break;
            
            case turnPhase.EnemyTurn:
                foreach (var unit in enemyUnits)
                {
                    var moveUnit = unit.GetComponent<MoveUnit>();
                    if (moveUnit != null)
                    {
                        if (moveUnit.moveActions == 0 && moveUnit.attackActions == 0)
                        {
                            currentTurnPhase = turnPhase.StartPlayerTurn;
                            continue;
                        }
                        else
                        {
                            // Enemy can still act with this unit, so we stay in EnemyTurn phase
                            return;
                        }
                    }
                }
                break;
        }
            



        turnPhaseText.text = currentTurnPhase.ToString();
    }
}
