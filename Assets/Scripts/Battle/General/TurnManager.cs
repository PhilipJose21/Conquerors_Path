using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [Header("Turn Indicator UI Icons")]
    [SerializeField] private Image playerIcon;
    [SerializeField] private Image enemyIcon;

    [Header("Active Icon Colors")]
    [SerializeField] private Color playerActiveColor = new Color(0.2f, 0.6f, 1f, 1f); 
    [SerializeField] private Color enemyActiveColor = new Color(1f, 0.2f, 0.2f, 1f); 

    [Header("Inactive Icon Color")]
    [SerializeField] private Color inactiveColor = new Color(0.3f, 0.3f, 0.3f, 0.6f); 

    [Header("Audio Settings")]
    [SerializeField] private AudioClip turnSwitchSFX;

    private Coroutine transitionCoroutine;
    private bool transitionPending = false;
    private bool isEnemyTurnProcessing = false;
    private PlayerData playerData;
    private PlayerBattleSO playerBattleData;
    private PlayerSO playerSO;
    
    void Awake()
    {
        buildingSystem = Object.FindFirstObjectByType<BuildingSystem>();
        playerData = Object.FindFirstObjectByType<PlayerData>();
        if (playerData != null)
        {
            playerBattleData = playerData.playerBattleSO;
            playerSO = playerData.playerSO;
        }
        gameOverScreen.SetActive(false);
        victoryScreen.SetActive(false);
    }

    void Start()
    {
        currentTurnPhase = turnPhase.SetupTurn;
        UpdateTurnUI();
    }

    void Update()
    {
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
                RecordCompletedLevel();
                victoryScreen.SetActive(true);
                HideTurnScreens(); // Clean up UI instantly
                return;
            }
        }

        if (currentTurnPhase != turnPhase.PlayerWin && currentTurnPhase != turnPhase.EnemyWin)
        {
            checkTurnPhase();
        }
    }

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

    private void RecordCompletedLevel()
    {
        if (playerBattleData == null || playerBattleData.currentLevel == null)
        {
            return;
        }

        ReturnTroopsToInventory();

        playerBattleData.currentLevel.isCompleted = true;
        playerBattleData.currentLevel.isUnlocked = true;
        if (!playerBattleData.completedLevels.Contains(playerBattleData.currentLevel))
        {
            playerBattleData.completedLevels.Add(playerBattleData.currentLevel);
        }

        if (!playerBattleData.unlockedLevels.Contains(playerBattleData.currentLevel))
        {
            playerBattleData.unlockedLevels.Add(playerBattleData.currentLevel);
        }
        if (!playerBattleData.currentLevel.rewardClaimed)
        {
            playerSO.energyPoints += playerBattleData.currentLevel.energyPointsReward;
            Debug.Log("Rewarded " + playerBattleData.currentLevel.energyPointsReward + " energy points.");
            playerSO.gems += playerBattleData.currentLevel.gemsReward;
            Debug.Log("Rewarded " + playerBattleData.currentLevel.gemsReward + " gems.");
            playerSO.coins += playerBattleData.currentLevel.coinsReward;
            Debug.Log("Rewarded " + playerBattleData.currentLevel.coinsReward + " coins.");
            playerBattleData.currentLevel.rewardClaimed = true;
        }
        
        playerBattleData.currentLevel = null;
    }

    private void ReturnTroopsToInventory()
    {
        if (playerUnits == null || playerUnits.Length == 0) return;
        if (playerBattleData == null || playerBattleData.playerUnitStats == null) return;

        foreach (GameObject unit in playerUnits)
        {
            if (unit == null) continue;

            MoveUnit moveUnitComp = unit.GetComponentInChildren<MoveUnit>() ?? unit.GetComponentInParent<MoveUnit>();
            
            if (moveUnitComp != null && moveUnitComp.unitData != null)
            {
                UnitSO survivingUnitData = moveUnitComp.unitData;
                playerBattleData.playerUnitStats.Add(survivingUnitData);
                Debug.Log($"[Inventory] Returned surviving unit to inventory: {survivingUnitData.unitName}");
            }
        }

        if (playerData != null)
        {
            playerData.updateUnitList();
        }
    }

    public void checkTurnPhase()
    {
        switch (currentTurnPhase)
        {
            case turnPhase.SetupTurn:
                if (buildingSystem != null)
                {
                    buildingSystem.gameObject.SetActive(true);
                }

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
        if (currentTurnPhase == turnPhase.PlayerWin || currentTurnPhase == turnPhase.EnemyWin)
        {
            HideTurnScreens();
            yield break;
        }

        PlayTurnSwitchSFX();

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

        if (currentTurnPhase == turnPhase.PlayerWin || currentTurnPhase == turnPhase.EnemyWin)
        {
            HideTurnScreens();
            yield break;
        }

        playerTurnScreen.SetActive(false);
        enemyTurnScreen.SetActive(false);
        currentTurnPhase = newPhase;

        UpdateTurnUI();

        transitionCoroutine = null;
        transitionPending = false;
    }

    private void UpdateTurnUI()
    {
        bool isPlayerTurn = (currentTurnPhase == turnPhase.PlayerTurn || currentTurnPhase == turnPhase.StartPlayerTurn);
        bool isEnemyTurn = (currentTurnPhase == turnPhase.EnemyTurn || currentTurnPhase == turnPhase.StartEnemyTurn);

        if (playerIcon != null)
        {
            playerIcon.color = isPlayerTurn ? playerActiveColor : inactiveColor;
        }

        if (enemyIcon != null)
        {
            enemyIcon.color = isEnemyTurn ? enemyActiveColor : inactiveColor;
        }
    }

    private void PlayTurnSwitchSFX()
    {
        if (turnSwitchSFX != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(turnSwitchSFX);
        }
    }

    private System.Collections.IEnumerator EnemyTurnSequence()
    {
        for (int i = 0; i < enemyUnits.Length; i++)
        {
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

                    // 1. Wait a frame for Act() to initialize the move coroutine
                    yield return null; 

                    // 2. Wait until the unit stops moving (isMoving becomes false)
                    yield return new WaitUntil(() => !moveUnit.isMoving);

                    // 3. Optional short buffer pause between actions for visual clarity
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }
        
        if (currentTurnPhase != turnPhase.PlayerWin && currentTurnPhase != turnPhase.EnemyWin)
        {
            currentTurnPhase = turnPhase.StartPlayerTurn;
        }
        isEnemyTurnProcessing = false;
    }

    public void endSetup(GameObject endSetupButton)
    {
        updateUnitLists();
        if (playerUnits.Length > 0)
        {
            placementPhase = false;
            Destroy(endSetupButton);
        }
    }

    public void updateUnitLists()
    {
        playerUnits = GameObject.FindGameObjectsWithTag("PlayerUnit");
        enemyUnits = GameObject.FindGameObjectsWithTag("EnemyUnit");
    }
}