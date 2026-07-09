using UnityEngine;
using TMPro;

public class ReinforcementCostUpdate : MonoBehaviour
{
    private PlayerBattleSO playerBattleSO;
    private BuildingSystem buildingSystem;
    public int unitReinforcementCost;
    public TextMeshProUGUI reinforcementCostText;
    void Awake()
    {
        playerBattleSO = Object.FindFirstObjectByType<PlayerData>().playerBattleSO;
        buildingSystem = Object.FindFirstObjectByType<BuildingSystem>();
        if (playerBattleSO.currentLevel == null)
        {
            Debug.LogWarning("Current level is not set in PlayerBattleSO.");
            return;
        }
        unitReinforcementCost = playerBattleSO.currentLevel.levelReinforcementCost + playerBattleSO.playerReinforcementCost;
    }

    void Update()
    {
        reinforcementCostText.text = unitReinforcementCost.ToString();
    }
}
