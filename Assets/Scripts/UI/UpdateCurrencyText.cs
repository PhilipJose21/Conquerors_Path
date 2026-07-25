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

        if (woodText != null) woodText.text = Mathf.Max(0, playerSO.woodResources).ToString("N0");
        if (stoneText != null) stoneText.text = Mathf.Max(0, playerSO.stoneResources).ToString("N0");
        if (farmText != null) farmText.text = Mathf.Max(0, playerSO.farmResources).ToString("N0");
        if (energyText != null) energyText.text = Mathf.Max(0, playerSO.energyPoints).ToString("N0");
        if (researchText != null) researchText.text = Mathf.Max(0, playerSO.researchPoints).ToString("N0");
        if (gemsText != null) gemsText.text = Mathf.Max(0, playerSO.gems).ToString("N0");
        if (coinsText != null) coinsText.text = Mathf.Max(0, playerSO.coins).ToString("N0");
    }
}