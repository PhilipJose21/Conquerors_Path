using UnityEngine;
using UnityEngine.UI;

public class ResearchLineUI : MonoBehaviour
{
    [Header("Target Connection")]
    [Tooltip("Drag the UI Node Button that this line leads TO into this slot.")]
    public ResearchNodeUI targetNode;

    private Image lineImage;

    private void Awake()
    {
        lineImage = GetComponent<Image>();
    }

    private void Start()
    {
        UpdateLineVisual();
    }

    // This will be called whenever the tree state refreshes
    public void UpdateLineVisual()
    {
        if (lineImage == null || targetNode == null || targetNode.nodeData == null) return;

        // The line should always match the exact visual state color of the node it unlocks!
        lineImage.color = targetNode.outerRingImage.color;
    }
}