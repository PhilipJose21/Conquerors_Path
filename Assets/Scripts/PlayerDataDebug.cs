using UnityEngine;
using TMPro;

public class PlayerDataDebug : MonoBehaviour
{
    public PlayerSO playerData;
    public PlayerBattleSO playerBattleData;
    private TextMeshProUGUI playerDataText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerDataText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerData == null && playerBattleData != null)
        {
            playerDataText.text = $"playerUnits: {string.Join(", ", playerBattleData.playerUnits)}\n" +
                $"playerUnitStats: {string.Join(", ", playerBattleData.playerUnitStats)}\n";
        }
        else if (playerData != null && playerBattleData == null)
        {
            playerDataText.text = $"Player Data:\n" +
                $"Wood: {playerData.woodResources}\n" +
                $"Stone: {playerData.stoneResources}\n" +
                $"Farm: {playerData.farmResources}\n" +
                $"Energy Points: {playerData.energyPoints}\n" +
                $"Research Points: {playerData.researchPoints}\n" +
                $"Gems: {playerData.gems}\n" +
                $"Coins: {playerData.coins}\n" +
                $"Unlocked Units: {string.Join(", ", playerData.unlockedUnits)}\n";
        }
        // else if (playerData != null && playerBattleData != null)
        // {
        //     playerDataText.text = $"Player Data:\n" +
        //         $"Wood: {playerData.woodResources}\n" +
        //         $"Stone: {playerData.stoneResources}\n" +
        //         $"Farm: {playerData.farmResources}\n" +
        //         $"Energy Points: {playerData.energyPoints}\n" +
        //         $"Research Points: {playerData.researchPoints}\n" +
        //         $"Gems: {playerData.gems}\n" +
        //         $"Coins: {playerData.coins}\n\n" +

        //         $"Player Battle Data:\n" +
        //         $"Health: {playerBattleData.healthPoints}\n" +
        //         $"Attack Damage: {playerBattleData.attackDamage}\n" +
        //         $"Attack Speed: {playerBattleData.attackSpeed}\n";
        // }
    }
}
