using UnityEngine;
using TMPro;
using System.Collections;

public class ActionText : MonoBehaviour
{
    public static ActionText Instance { get; private set; }

    [Header("Setup")]
    [Tooltip("Template TextMeshProUGUI that gets cloned for each popup. Can be inactive.")]
    public TextMeshProUGUI actionText;
    [Tooltip("Parent transform popups are spawned under (usually a Canvas or UI Container with Vertical Layout Group).")]
    public Transform actionTextTransform;

    [Header("Behaviour")]
    public float lifetime = 2.5f;

    [Header("Colors")]
    public Color playerColor = new Color(0.2f, 0.6f, 1f); // Blue
    public Color enemyColor = new Color(0.9f, 0.2f, 0.2f);  // Red
    public Color harvestColor = new Color(0.3f, 0.8f, 1f);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void InstantiateActionText(Vector3 worldPosition, string message, Color color)
    {
        if (actionText == null) return;

        Transform parent = actionTextTransform != null ? actionTextTransform : transform;
        TextMeshProUGUI instance = Instantiate(actionText, parent);
        instance.gameObject.SetActive(true);
        instance.text = message;

        // Force full alpha
        color.a = 1f; 
        instance.color = color;

        // Do NOT set position here — Vertical Layout Group automatically positions it!

        StartCoroutine(AnimateAndDestroy(instance));
    }

    private IEnumerator AnimateAndDestroy(TextMeshProUGUI instance)
    {
        float solidTime = 2.0f; // Stay solid for 2 seconds
        float fadeTime = 0.5f;  // Fade out over 0.5 seconds
        
        // Wait solid phase
        yield return new WaitForSeconds(solidTime);

        // Fade phase
        float fadeElapsed = 0f;
        Color startColor = instance.color;

        while (fadeElapsed < fadeTime)
        {
            fadeElapsed += Time.deltaTime;
            float t = fadeElapsed / fadeTime;

            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0f, t);
            instance.color = c;

            yield return null;
        }

        if (instance != null)
        {
            Destroy(instance.gameObject);
        }
    }

    public void ShowAttackText(string attackerName, string targetName, int damage, Vector3 worldPosition, bool isPlayer)
    {
        string message = $"{attackerName} hit {targetName} for {damage} damage!";
        Color chosenColor = isPlayer ? playerColor : enemyColor;
        InstantiateActionText(worldPosition, message, chosenColor);
    }

    public void ShowAttackText(string attackerName, string targetName, int damage, Vector3 worldPosition)
    {
        ShowAttackText(attackerName, targetName, damage, worldPosition, false);
    }

    public void ShowHarvestText(string harvesterName, int amount, Vector3 worldPosition)
    {
        string message = $"{harvesterName} harvested {amount}!";
        InstantiateActionText(worldPosition, message, harvestColor);
    }
}