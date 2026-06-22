using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResearchNodeUI : MonoBehaviour
{
    public ResearchNodeData nodeData;
    private ResearchTreeManager manager;
    
    [Header("UI Component Links")]
    public Image outerRingImage;
    public TextMeshProUGUI displayNameText;
    
    [Header("Visual State Colors")]
    public Color unlockedColor = Color.green;
    public Color availableColor = Color.black;
    public Color lockedColor = Color.red;

    private void Start()
    {
        manager = FindFirstObjectByType<ResearchTreeManager>();
        
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(OnNodeClicked);
        }

        if (manager != null)
        {
            manager.RefreshAllTreeNodes();
        }
    }

    public void OnNodeClicked()
    {
        if (manager != null && nodeData != null)
        {
            // Sends the data AND where this button is sitting on the screen!
            manager.SelectNode(nodeData, this.transform.position);
        }
    }

    public void UpdateNodeVisuals(System.Func<string, bool> checkUnlocked, System.Func<ResearchNodeData[], bool> checkPrereqs)
    {
        if (nodeData == null) return;
        
        if (displayNameText != null)
            displayNameText.text = nodeData.nodeDisplayName;

        bool isUnlocked = checkUnlocked(nodeData.nodeId) || nodeData.isUnlockedByDefault;
        bool isAvailable = !isUnlocked && checkPrereqs(nodeData.prerequisites);

        if (isUnlocked)
        {
            outerRingImage.color = unlockedColor;
        }
        else if (isAvailable)
        {
            outerRingImage.color = availableColor;
        }
        else
        {
            outerRingImage.color = lockedColor;
        }
    }
}