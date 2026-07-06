using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu()]
public class LevelSO : ScriptableObject
{
    public WorldSO levelName;
    public Sprite levelImage;
    public int level;
}
