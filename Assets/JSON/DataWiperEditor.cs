#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

public static class DataWiperEditor
{
    // The attribute belongs strictly here on the public static void method!
    [MenuItem("Tools/Conquerors Path/Wipe All Progress")]
    public static void CompletelyWipeSaveData()
    {
        // 1. Clear out memory configurations
        PlayerPrefs.DeleteAll();

        // 2. Locate and delete the physical JSON disk save file
        string path = Path.Combine(Application.persistentDataPath, "conquerors_path_save.json");
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"<color=red>Successfully Deleted JSON Save File at:</color> {path}");
        }
        else
        {
            Debug.Log("No JSON file found. Your disk save state is already empty!");
        }

        Debug.Log("<color=green><b>All game progress completely wiped! Ready for testing.</b></color>");
    }
}
#endif