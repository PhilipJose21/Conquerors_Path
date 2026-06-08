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

    private PlayerData playerData;
    private PlayerSO playerSO;

    void Awake()
    {
        playerData = UnityEngine.Object.FindFirstObjectByType<PlayerData>();
        if (playerData == null)
        {
            Debug.LogWarning("UpdateCurrencyText: Could not find PlayerData in the scene.");
            return;
        }

        playerSO = playerData.playerSO;
        if (playerSO == null)
        {
            Debug.LogWarning("UpdateCurrencyText: PlayerData does not have a PlayerSO assigned.");
        }
    }

    void Update()
    {
        if (playerSO == null)
            return;

        if (woodText != null) woodText.text = playerSO.woodResources.ToString();
        if (stoneText != null) stoneText.text = playerSO.stoneResources.ToString();
        if (farmText != null) farmText.text = playerSO.farmResources.ToString();
        if (energyText != null) energyText.text = playerSO.energyPoints.ToString();
        if (researchText != null) researchText.text = playerSO.researchPoints.ToString();
        if (gemsText != null) gemsText.text = playerSO.gems.ToString();
        if (coinsText != null) coinsText.text = playerSO.coins.ToString();
    }
}
