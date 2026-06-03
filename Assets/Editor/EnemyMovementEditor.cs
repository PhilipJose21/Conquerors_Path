using UnityEditor;
using UnityEngine;
using Battle = UnityEngine;

[CustomEditor(typeof(EnemyMovement))]
public class EnemyMovementEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EnemyMovement em = (EnemyMovement)target;
        if (GUILayout.Button("Force Act (move now)"))
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Force Act only works in Play mode.");
            }
            else
            {
                em.ForceAct();
            }
        }
    }
}
