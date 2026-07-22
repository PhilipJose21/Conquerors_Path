using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu()]
public class PlayerSO : ScriptableObject
{
    public int woodResources;
    public int stoneResources;
    public int farmResources;
    public int energyPoints;
    public int researchPoints;
    public int gems;
    public int coins;
    public List<UnitSO> unlockedUnits;
    public Sprite woodIcon;
    public Sprite stoneIcon;
    public Sprite farmIcon;
    public Sprite energyIcon;
    public Sprite researchIcon;
    public Sprite gemsIcon;
    public Sprite coinsIcon;
}
