using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Building))]
[CanEditMultipleObjects]
public class BuildingEditor : Editor
{
    SerializedProperty manualOffsetProp;

    private void OnEnable()
    {
        manualOffsetProp = serializedObject.FindProperty("manualOffset");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        serializedObject.Update();
        EditorGUILayout.LabelField("Manual Position Offset", EditorStyles.boldLabel);
        Vector3 off = manualOffsetProp.vector3Value;
        off.x = EditorGUILayout.Slider("Offset X", off.x, -5f, 5f);
        off.y = EditorGUILayout.Slider("Offset Y", off.y, -5f, 5f);
        off.z = EditorGUILayout.Slider("Offset Z", off.z, -5f, 5f);
        manualOffsetProp.vector3Value = off;
        serializedObject.ApplyModifiedProperties();
    }
}

[CustomEditor(typeof(BuildingPreview))]
[CanEditMultipleObjects]
public class BuildingPreviewEditor : Editor
{
    SerializedProperty manualOffsetProp;

    private void OnEnable()
    {
        manualOffsetProp = serializedObject.FindProperty("manualOffset");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        serializedObject.Update();
        EditorGUILayout.LabelField("Manual Position Offset", EditorStyles.boldLabel);
        Vector3 off = manualOffsetProp.vector3Value;
        off.x = EditorGUILayout.Slider("Offset X", off.x, -5f, 5f);
        off.y = EditorGUILayout.Slider("Offset Y", off.y, -5f, 5f);
        off.z = EditorGUILayout.Slider("Offset Z", off.z, -5f, 5f);
        manualOffsetProp.vector3Value = off;
        serializedObject.ApplyModifiedProperties();
    }
}
