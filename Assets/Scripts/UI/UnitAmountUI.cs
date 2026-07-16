using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UnitAmountUI : MonoBehaviour
{
    private TextMeshProUGUI amountText;
    private PlayerData playerData;
    private PlayerBattleSO playerBattleSO;
    [SerializeField] private UnitSO assignedUnit;

    void Awake()
    {
        amountText = this.GetComponent<TextMeshProUGUI>();
        playerData = FindFirstObjectByType<PlayerData>();
        playerBattleSO = playerData != null ? playerData.playerBattleSO : null;
    }

    void Update()
    {
        if (amountText == null || playerBattleSO == null || assignedUnit == null)
        {
            return;
        }

        List<UnitSO> unitList = playerBattleSO.playerUnitStats;
        int unitCount = 0;

        if (unitList != null)
        {
            foreach (UnitSO unit in unitList)
            {
                if (unit == assignedUnit)
                {
                    unitCount++;
                }
            }
        }

        amountText.text = unitCount.ToString();

    }
}
