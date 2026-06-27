using UnityEngine;
using System.Collections.Generic;

public class TroopList : MonoBehaviour
{
    private PlayerData playerData;
    private PlayerSO playerSO;
    private PlayerBattleSO playerBattleSO;

    public TrainUpgradeTroopUI trainUpgradeTroopUI;
    //units the player can unlock and train
    public List<UnitSO> unitList;

    //units the player currently has
    public List<UnitSO> playerUnits;

    void Awake()
    {
        playerData = Object.FindFirstObjectByType<PlayerData>();
        playerSO = playerData.playerSO;
        playerBattleSO = playerData.playerBattleSO;
        trainUpgradeTroopUI = Object.FindFirstObjectByType<TrainUpgradeTroopUI>();

    }
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        updateUnitLists();
    }

    public void updateUnitLists()
    {
        playerUnits = playerBattleSO.playerUnitStats;
    }

    public void addUnit(int index)
    {

    }

    
}
