using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public PlayerSO playerSO;
    public PlayerBattleSO playerBattleSO;
    public int playerWood;
    public int playerStone;
    public int playerFarm;
    public int playerGold;

    void Update()
    {
        playerWood = playerBattleSO.woodHarvestAmount;
        playerStone = playerBattleSO.stoneHarvestAmount;
        playerFarm = playerBattleSO.farmHarvestAmount;
        playerGold = playerBattleSO.goldHarvestAmount;
    }
}
