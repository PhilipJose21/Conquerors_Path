using UnityEngine;

[CreateAssetMenu(fileName = "NewResearchNode", menuName = "Research/Node")]
public class ResearchNodeData : ScriptableObject
{
    [Header("UI Display Settings")]
    public string nodeId;
    public string nodeDisplayName;
    [TextArea(3, 5)] public string description;
    public int cost;

    [Header("Path Prerequisites")]
    public bool isUnlockedByDefault = false;
    public ResearchNodeData[] prerequisites;

    [Header("Game Modifiers")]
    [Tooltip("Add one or more effects to this node. Perfect for multi-effect upgrades like Marathon Training.")]
    public UpgradeEffect[] upgradeEffects; // Supports multiple modifiers on a single node

    [System.Serializable] 
    public struct UpgradeEffect
    {
        public UpgradeType targetUpgrade;
        public float modifierValue;
    }

    public enum UpgradeType
    {
        None,
        SupportOutput,
        SupportHP,
        RangerRange,
        RangerDamage,
        ScoutMobility,
        MeleeDamage,
        MeleeHP,
        MeleeMobility
    }
}