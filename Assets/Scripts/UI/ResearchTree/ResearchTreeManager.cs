using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; 

public class ResearchTreeManager : MonoBehaviour
{
    [Header("Player Inventory Economy")]
    public int currentResearchPoints = 123; 

    [Header("Research Points HUD UI Link")]
    public TextMeshProUGUI researchPointsText;

    [Header("Inspection Bubble UI Links")]
    public GameObject inspectionDeckPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;
    public Button unlockButton;

    [Header("Tracked Progression States")]
    private HashSet<string> unlockedNodeIds = new HashSet<string>();
    private ResearchNodeData currentlySelectedNode;

    private void Awake()
    {
        unlockedNodeIds.Add("start"); 
        if (inspectionDeckPanel != null) inspectionDeckPanel.SetActive(false);
    }

    private void Start()
    {
        // Initialize the HUD counter text at startup
        UpdateCurrencyHUD();
    }

    public bool IsNodeUnlocked(string nodeId)
    {
        return unlockedNodeIds.Contains(nodeId);
    }

    public bool ArePrerequisitesMet(ResearchNodeData[] prerequisites)
    {
        if (prerequisites == null || prerequisites.Length == 0) return true;
        foreach (var prereq in prerequisites)
        {
            if (prereq != null && !unlockedNodeIds.Contains(prereq.nodeId)) return false;
        }
        return true;
    }

    public void SelectNode(ResearchNodeData nodeData, Vector3 nodeScreenPosition)
    {
        if (nodeData == null) return;
        currentlySelectedNode = nodeData;

        if (inspectionDeckPanel != null)
        {
            inspectionDeckPanel.SetActive(true);
            Vector3 positionOffset = new Vector3(240f, -80f, 0f); 
            inspectionDeckPanel.transform.position = nodeScreenPosition + positionOffset;
        }

        if (titleText != null) titleText.text = nodeData.nodeDisplayName;
        if (descriptionText != null) descriptionText.text = nodeData.description;
        if (costText != null) costText.text = $"{nodeData.cost}"; 
    }

    public void BuySelectedNode()
    {
        if (currentlySelectedNode == null) return;

        // 💡 SAFETY CHECK: If the node is ALREADY unlocked, STOP immediately!
        if (IsNodeUnlocked(currentlySelectedNode.nodeId) || currentlySelectedNode.isUnlockedByDefault)
        {
            Debug.LogWarning($"Node {currentlySelectedNode.nodeDisplayName} is already unlocked!");
            return; 
        }

        // Existing validation checks...
        if (currentResearchPoints >= currentlySelectedNode.cost && ArePrerequisitesMet(currentlySelectedNode.prerequisites))
        {
            currentResearchPoints -= currentlySelectedNode.cost;
            unlockedNodeIds.Add(currentlySelectedNode.nodeId);
            
            UpdateCurrencyHUD();
            RefreshAllTreeNodes();
            CloseInspectionBubble(); 
        }
        else
        {
            Debug.LogWarning("Cannot unlock! Not enough points or missing prerequisites.");
        }
    }

    // Helper function to keep our screen counter synchronized
    public void UpdateCurrencyHUD()
    {
        if (researchPointsText != null)
        {
            researchPointsText.text = currentResearchPoints.ToString();
        }
    }

    public void CloseInspectionBubble()
    {
        if (inspectionDeckPanel != null)
        {
            inspectionDeckPanel.SetActive(false);
        }
    }

    public void RefreshAllTreeNodes()
    {
        ResearchNodeUI[] allNodes = FindObjectsByType<ResearchNodeUI>(FindObjectsSortMode.None);
        foreach (var nodeUI in allNodes)
        {
            nodeUI.UpdateNodeVisuals(IsNodeUnlocked, ArePrerequisitesMet);
        }

        ResearchLineUI[] allLines = FindObjectsByType<ResearchLineUI>(FindObjectsSortMode.None);
        foreach (var lineUI in allLines)
        {
            lineUI.UpdateLineVisual();
        }
    }
    public void LoadTargetScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}