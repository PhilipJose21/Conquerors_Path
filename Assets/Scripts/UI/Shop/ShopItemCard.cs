using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemCard : MonoBehaviour
{
    // Added UnitCard to the type choices
    public enum ItemType { CoinsPack, GemsPack, UnitCard }

    [Header("Item Core Data")]
    [SerializeField] private ItemType cardType;
    [SerializeField] private string itemName = "Archer";
    [SerializeField] private string itemDescription = "High long-range damage.";
    [SerializeField] private Sprite itemIcon;
    
    [Header("Unit Only Configuration")]
    [SerializeField] private UnitSO unitData; // Drag the matching UnitSO asset file here!

    [Header("Cost Configuration")]
    [SerializeField] private int rewardAmount = 5000; // Used for Coins/Gems rewards
    [SerializeField] private int gemCost = 50;       // Used as the Gem price for Units
    [SerializeField] private string realMoneyPriceTag = "$0.99"; // Used for Packs

    [Header("Internal UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costButtonText;
    [SerializeField] private Image cardIconImage;

    private ShopManager shopManager;
    private Button purchaseButton;

    private void OnEnable()
    {
        if (shopManager == null)
        {
            shopManager = Object.FindAnyObjectByType<ShopManager>();
        }

        if (shopManager != null)
        {
            shopManager.OnUnlockedUnitsChanged -= RefreshPurchaseState;
            shopManager.OnUnlockedUnitsChanged += RefreshPurchaseState;
        }

        RefreshPurchaseState();
    }

    private void OnDisable()
    {
        if (shopManager != null)
        {
            shopManager.OnUnlockedUnitsChanged -= RefreshPurchaseState;
        }
    }

    void Start()
    {
        shopManager = Object.FindAnyObjectByType<ShopManager>();
        purchaseButton = GetComponentInChildren<Button>();

        // Push baseline descriptions to the text elements
        if (titleText != null) titleText.text = itemName;
        if (descriptionText != null) descriptionText.text = itemDescription;

        UpdateCostButtonLabel();

        // Apply custom art sprite to card face layout
        if (cardIconImage != null && itemIcon != null)
        {
            cardIconImage.sprite = itemIcon;
        }

        if (purchaseButton != null && shopManager != null)
        {
            purchaseButton.onClick.AddListener(TriggerPurchase);
        }

        RefreshPurchaseState();
    }

    private void TriggerPurchase()
    {
        if (shopManager == null) return;

        if (cardType == ItemType.CoinsPack)
        {
            shopManager.BuyCoinsPack(rewardAmount);
        }
        else if (cardType == ItemType.GemsPack)
        {
            shopManager.BuyGemsPack(rewardAmount);
        }
        else if (cardType == ItemType.UnitCard)
        {
            // Safety check to ensure you assigned a UnitSO asset file to this card!
            if (unitData != null)
            {
                shopManager.BuySpecialUnit(unitData, gemCost);
            }
            else
            {
                Debug.LogError($"[Shop Error] {gameObject.name} is set to UnitCard but 'Unit Data' slot is empty!");
            }
        }
    }

    private void RefreshPurchaseState()
    {
        if (purchaseButton == null) return;

        if (cardType != ItemType.UnitCard)
        {
            purchaseButton.interactable = true;
            UpdateCostButtonLabel();
            return;
        }

        bool alreadyUnlocked = shopManager != null && shopManager.IsUnitUnlocked(unitData);
        purchaseButton.interactable = !alreadyUnlocked;

        if (alreadyUnlocked)
        {
            if (costButtonText != null) costButtonText.text = "Unlocked";
            return;
        }

        UpdateCostButtonLabel();
    }

    private void UpdateCostButtonLabel()
    {
        if (costButtonText == null) return;

        if (cardType == ItemType.UnitCard)
        {
            costButtonText.text = $"{gemCost} Gems";
        }
        else
        {
            costButtonText.text = realMoneyPriceTag;
        }
    }
}