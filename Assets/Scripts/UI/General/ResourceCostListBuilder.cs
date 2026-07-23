using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class ResourceCostListBuilder
{
    public static void Build(BuildingData buildingData, PlayerSO playerSO, GameObject costPrefab, Transform costParent, int multiplier = 1)
    {
        if (buildingData == null || playerSO == null || costPrefab == null || costParent == null)
        {
            Debug.LogWarning("ResourceCostListBuilder.Build: Missing required reference.");
            return;
        }

        for (int i = costParent.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(costParent.GetChild(i).gameObject);
        }

        for (int i = 0; i < 4; i++)
        {
            int costValue = 0;
            Sprite icon = null;

            switch (i)
            {
                case 0: costValue = buildingData.woodCost * multiplier; icon = playerSO.woodIcon; break;
                case 1: costValue = buildingData.rockCost * multiplier; icon = playerSO.stoneIcon; break;
                case 2: costValue = buildingData.farmCost * multiplier; icon = playerSO.farmIcon; break;
                case 3: costValue = buildingData.coinCost * multiplier; icon = playerSO.coinsIcon; break;
            }

            if (costValue <= 0) continue;

            GameObject entry = Object.Instantiate(costPrefab, costParent);
            TextMeshProUGUI costText = entry.GetComponentInChildren<TextMeshProUGUI>();
            Image costImage = entry.GetComponentInChildren<Image>();

            if (costText != null) costText.text = costValue.ToString();
            if (costImage != null) costImage.sprite = icon;
        }
    }
}