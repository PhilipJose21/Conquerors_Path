using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))] 
public class WorldObjectAudioFeedback : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("Drag the selection sound effect for this specific building prefab here.")]
    [SerializeField] private AudioClip selectionSound;

    // Unity automatically calls this when a player clicks the attached collider
    void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (selectionSound != null)
        {
            SoundManager.Instance?.PlayClickSFX(selectionSound);
        }
    }
}