using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("Main Shop Panel")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button openShopButton; 
    [SerializeField] private Button closeButton;

    [Header("Tab Navigation Buttons")]
    [SerializeField] private Button unitsButton;
    [SerializeField] private Button coinsButton;
    [SerializeField] private Button gemsButton;

    [Header("Sub Display Scroll Panels")]
    [SerializeField] private GameObject unitsShopPanel;
    [SerializeField] private GameObject coinsShopPanel;
    [SerializeField] private GameObject gemsShopPanel;

    [Header("Top Bar UI HUD Displays")]
    [SerializeField] private TextMeshProUGUI goldCounterText;
    [SerializeField] private TextMeshProUGUI gemsCounterText;

    [Header("Tab Visual Feedback")]
    [Range(0f, 1f)] 
    [SerializeField] private float inactiveAlphaOrDim = 0.4f; 

    private PlayerData playerDataObj;
    private PlayerSO playerData;

    private Color originalUnitsColor;
    private Color originalCoinsColor;
    private Color originalGemsColor;
    private bool colorsCached = false;

    public System.Action OnUnlockedUnitsChanged;

    void Awake()
    {
        playerDataObj = Object.FindFirstObjectByType<PlayerData>();
        if (playerDataObj != null)
        {
            playerData = playerDataObj.playerSO;
        }
        else
        {
            Debug.LogWarning("ShopManager: Could not find PlayerData in the scene.");
        }
    }

    void Start()
    {
        if (openShopButton != null) openShopButton.onClick.AddListener(ToggleShopPanel);
        
        if (unitsButton != null) unitsButton.onClick.AddListener(OpenUnitsShop);
        if (coinsButton != null) coinsButton.onClick.AddListener(OpenCoinsShop);
        if (gemsButton != null)  gemsButton.onClick.AddListener(OpenGemsShop);
        if (closeButton != null) closeButton.onClick.AddListener(CloseShopPanel);
        
        OpenUnitsShop();
    }

    void Update()
    {
        if (playerData == null) return;

        if (goldCounterText != null) goldCounterText.text = playerData.coins.ToString("N0");
        if (gemsCounterText != null) gemsCounterText.text = playerData.gems.ToString("N0");
    }

    public void ToggleShopPanel()
    {
        if (shopPanel == null) return;

        if (shopPanel.activeSelf)
        {
            CloseShopPanel();
        }
        else
        {
            OpenShopPanel();
        }
    }
    
    public void BuySpecialUnit(UnitSO unitToUnlock, int gemCost)
    {
        if (playerData == null || unitToUnlock == null) return;

        if (IsUnitUnlocked(unitToUnlock))
        {
            Debug.Log($"{unitToUnlock.name} is already unlocked.");
            return;
        }

        if (playerData.gems >= gemCost)
        {
            playerData.gems -= gemCost;

            if (playerData.unlockedUnits == null)
            {
                playerData.unlockedUnits = new System.Collections.Generic.List<UnitSO>();
            }

            playerData.unlockedUnits.Add(unitToUnlock);

            OnUnlockedUnitsChanged?.Invoke();
            Debug.Log($"Successfully unlocked {unitToUnlock.name}!");
        }
        else
        {
            Debug.LogWarning("Not enough Gems to buy this unit!");
        }
    }

    public bool IsUnitUnlocked(UnitSO unit)
    {
        if (playerData == null || unit == null) return false;
        if (playerData.unlockedUnits == null) return false;

        return playerData.unlockedUnits.Contains(unit);
    }

    public void BuyCoinsPack(int coinsReward)
    {
        if (playerData == null) return;
        
        playerData.coins += coinsReward;
        Debug.Log($"[Real Money Mockup] Direct purchase success! Added {coinsReward} Coins.");
    }

    public void BuyGemsPack(int gemsReward)
    {
        if (playerData == null) return;

        playerData.gems += gemsReward;
        Debug.Log($"Added {gemsReward} Gems to player profile!");
    }

    public void OpenShopPanel()
    {
        shopPanel.SetActive(true);
    }

    public void CloseShopPanel()
    {
        shopPanel.SetActive(false);
    }

    public void OpenUnitsShop()
    {
        if (unitsShopPanel != null) unitsShopPanel.SetActive(true);
        if (coinsShopPanel != null) coinsShopPanel.SetActive(false);
        if (gemsShopPanel != null)  gemsShopPanel.SetActive(false);

        SetActiveTabVisuals(unitsButton);
    }

    public void OpenCoinsShop()
    {
        if (unitsShopPanel != null) unitsShopPanel.SetActive(false);
        if (coinsShopPanel != null) coinsShopPanel.SetActive(true);
        if (gemsShopPanel != null)  gemsShopPanel.SetActive(false);

        SetActiveTabVisuals(coinsButton);
    }

    public void OpenGemsShop()
    {
        if (unitsShopPanel != null) unitsShopPanel.SetActive(false);
        if (coinsShopPanel != null) coinsShopPanel.SetActive(false);
        if (gemsShopPanel != null)  gemsShopPanel.SetActive(true);

        SetActiveTabVisuals(gemsButton);
    }

    private void CacheOriginalColors()
    {
        if (colorsCached) return;

        if (unitsButton != null) originalUnitsColor = unitsButton.image.color;
        if (coinsButton != null) originalCoinsColor = coinsButton.image.color;
        if (gemsButton != null)  originalGemsColor = gemsButton.image.color;

        colorsCached = true;
    }

    private void SetActiveTabVisuals(Button activeBtn)
    {
        CacheOriginalColors();

        if (unitsButton != null) unitsButton.image.color = originalUnitsColor * new Color(inactiveAlphaOrDim, inactiveAlphaOrDim, inactiveAlphaOrDim, 1f);
        if (coinsButton != null) coinsButton.image.color = originalCoinsColor * new Color(inactiveAlphaOrDim, inactiveAlphaOrDim, inactiveAlphaOrDim, 1f);
        if (gemsButton != null)  gemsButton.image.color = originalGemsColor * new Color(inactiveAlphaOrDim, inactiveAlphaOrDim, inactiveAlphaOrDim, 1f);

        if (activeBtn == unitsButton && unitsButton != null) unitsButton.image.color = originalUnitsColor;
        if (activeBtn == coinsButton && coinsButton != null) coinsButton.image.color = originalCoinsColor;
        if (activeBtn == gemsButton && gemsButton != null)   gemsButton.image.color = originalGemsColor;
    }
}