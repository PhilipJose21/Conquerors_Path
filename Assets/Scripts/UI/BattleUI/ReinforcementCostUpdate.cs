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
        unitReinforcementCost = playerBattleSO.playerReinforcementCost;
    }

    void Update()
    {
        reinforcementCostText.text = unitReinforcementCost.ToString();
    }
}
