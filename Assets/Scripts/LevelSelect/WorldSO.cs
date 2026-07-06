using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[CreateAssetMenu()]
public class WorldSO : ScriptableObject
{
    public string worldName;
    public Sprite worldImage;
    public string worldLevelScene;
    public List<string> sceneNames;
    public List<LevelSO> levels;
}
