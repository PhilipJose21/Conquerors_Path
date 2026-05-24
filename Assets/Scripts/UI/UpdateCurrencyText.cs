using UnityEngine;
using TMPro;

public class UpdateCurrencyText : MonoBehaviour
{
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI stoneText;
    public TextMeshProUGUI farmText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI researchText;
    public TextMeshProUGUI gemsText;
    public TextMeshProUGUI coinsText;
    public PlayerData playerData => FindObjectOfType<PlayerData>();
    public PlayerSO playerSO => playerData.playerSO;

    void Update()
    {
        woodText.text = playerSO.woodResources.ToString();
        stoneText.text = playerSO.stoneResources.ToString();
        farmText.text = playerSO.farmResources.ToString();
        energyText.text = playerSO.energyPoints.ToString();
        researchText.text = playerSO.researchPoints.ToString();
        gemsText.text = playerSO.gems.ToString();
        coinsText.text = playerSO.coins.ToString();
    }
}
