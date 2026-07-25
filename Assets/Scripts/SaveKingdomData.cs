using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
	public int woodResources;
	public int stoneResources;
	public int farmResources;
	public int energyPoints;
	public int researchPoints;
	public int gems;
	public int coins;
	public List<string> unlockedUnitKeys = new List<string>();
	public List<string> trainedUnitKeys = new List<string>();
	public List<SavedLevelData> levelStates = new List<SavedLevelData>();
}

[Serializable]
public class SavedLevelData
{
	public string levelKey;
	public bool isUnlocked;
	public bool isCompleted;
	public bool rewardClaimed;
}

[Serializable]
public class SaveKingdomData
{
	public List<SavedBuildingData> buildings = new List<SavedBuildingData>();
}

[Serializable]
public class SavedBuildingData
{
	public string buildingKey;
	public Vector3 worldPosition;
	public float rootRotation;
	public float rotation;
	public List<Vector3> occupiedPositions = new List<Vector3>();
	public PassiveResourceSaveData passiveResource = new PassiveResourceSaveData();
}

[Serializable]
public class PassiveResourceSaveData
{
	public int level = 1;
	public bool isActive;
	public int resourceAmount;
	public float currentTime;
	public int coinCost;
	public int farmCost;
	public int rockCost;
	public int woodCost;
	public int gemCost;
	public int energyCost;
}

[Serializable]
public class GameSaveData
{
	public PlayerSaveData player = new PlayerSaveData();
	public SaveKingdomData kingdom = new SaveKingdomData();
}