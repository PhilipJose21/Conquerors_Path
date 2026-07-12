using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class KingdomSaveManager : MonoBehaviour
{
    public static KingdomSaveManager Instance { get; private set; }

    [SerializeField] private SaveKingdomData currentKingdom = new SaveKingdomData();
    [SerializeField] private PlayerSaveData currentPlayer = new PlayerSaveData();

    private const string SaveFileName = "conquerors_path_save.json";
    private PlayerSO registeredPlayerSO;
    private PlayerSO registeredDefaultPlayerSO;
    private PlayerBattleSO registeredPlayerBattleSO;
    private readonly Dictionary<string, UnitSO> unitRegistry = new Dictionary<string, UnitSO>();
    private readonly List<PlayerSaveData> playerDefaults = new List<PlayerSaveData>();
    private readonly List<LevelSnapshot> levelDefaults = new List<LevelSnapshot>();
    private readonly List<UnitSnapshot> unitDefaults = new List<UnitSnapshot>();
    private readonly List<SavedBuildingData> buildingSnapshots = new List<SavedBuildingData>();
    private bool hasLoadedFromDisk;
    // True only when we positively know there was no save file on disk (i.e. an actual
    // brand-new save). We must NOT infer "fresh save" from currentPlayer's values being
    // zero, since a real player can legitimately have 0 of every resource after spending
    // it all (e.g. right after training a troop).
    private bool isFreshSave;

    [Serializable]
    private class LevelSnapshot
    {
        public LevelSO levelSO;
        public bool isUnlocked;
        public bool isCompleted;
        public bool rewardClaimed;
    }

    [Serializable]
    private class UnitSnapshot
    {
        public UnitSO unitSO;
        public int level;
        public int health;
        public int damage;
        public int attackRange;
        public int mobility;
        public int movePoints;
        public int attackPoints;
        public int unitCost;
        public int harvestAmount;
    }

    public bool HasSavedKingdom => currentKingdom != null && currentKingdom.buildings.Count > 0;
    public bool IsSaveFileEmpty
    {
        get
        {
            string path = Path.Combine(Application.persistentDataPath, SaveFileName);
            return !File.Exists(path);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject managerObject = new GameObject(nameof(KingdomSaveManager));
        managerObject.AddComponent<KingdomSaveManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CaptureDefaults();

        SceneManager.sceneLoaded += HandleSceneLoaded;

        if (ShouldUseDiskPersistence())
        {
            LoadFromDisk();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;

        if (Instance == this)
        {
            Instance = null;
        }
    }

   public void SaveCurrentKingdom()
    {
        BuildingSystem buildingSystem = FindCurrentKingdomBuildingSystem();
        bool capturedFromSystem = false;

        if (buildingSystem != null)
        {
            CaptureFrom(buildingSystem);
            capturedFromSystem = true; // Mark that we successfully saved the whole state cleanly
        }

        // Only run this fallback if the main BuildingSystem wasn't available in the scene
        if (!capturedFromSystem)
        {
            RefreshPassiveResourceSnapshotsFromScene();
        }
        
        SyncCurrentKingdomFromSnapshots();

        CapturePlayerData();

        if (ShouldUseDiskPersistence())
        {
            SaveToDisk();
        }
    }

    public void ResetSaveData()
    {
        CaptureDefaults();
        RestoreDefaults();
        
        currentKingdom = new SaveKingdomData();
        buildingSnapshots.Clear(); 

        PlayerSaveData defaultPlayerSnapshot = playerDefaults.Count > 0
            ? playerDefaults[0]
            : CapturePlayerSnapshot(registeredDefaultPlayerSO != null ? registeredDefaultPlayerSO : registeredPlayerSO);
        currentPlayer = ClonePlayerSaveData(defaultPlayerSnapshot);
        isFreshSave = false;

        ApplyCapturedPlayerSnapshot(currentPlayer);

        if (ShouldUseDiskPersistence())
        {
            string path = GetSaveFilePath();
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    public void CaptureFrom(BuildingSystem buildingSystem)
    {
        if (buildingSystem == null || buildingSystem.isBattleScene)
        {
            return;
        }

        SaveKingdomData capturedKingdom = buildingSystem.CaptureKingdomState();
        
        // If it returned null, it means it aborted to safeguard your actual data
        if (capturedKingdom == null)
        {
            return; 
        }

        buildingSnapshots.Clear();

        if (capturedKingdom.buildings != null)
        {
            buildingSnapshots.AddRange(capturedKingdom.buildings);
        }

        SyncCurrentKingdomFromSnapshots();
    }

    public void RegisterPlacedBuilding(Building building, List<Vector3> occupiedPositions = null)
    {
        if (building == null)
        {
            return;
        }

        SavedBuildingData snapshot = CreateBuildingSnapshot(building, occupiedPositions);
        if (snapshot == null)
        {
            return;
        }

        int existingIndex = buildingSnapshots.FindIndex(existing => IsSameBuilding(existing, snapshot));
        if (existingIndex >= 0)
        {
            buildingSnapshots[existingIndex] = snapshot;
        }
        else
        {
            buildingSnapshots.Add(snapshot);
        }

        SyncCurrentKingdomFromSnapshots();
    }

    public void RemovePlacedBuilding(Building building)
    {
        if (building == null)
        {
            return;
        }

        buildingSnapshots.RemoveAll(snapshot => snapshot != null && snapshot.buildingKey == building.Name && ArePositionsClose(snapshot.worldPosition, building.transform.position));
        SyncCurrentKingdomFromSnapshots();
    }

    public void RegisterPlayerData(PlayerSO playerSO, PlayerBattleSO playerBattleSO, PlayerSO defaultPlayerSO = null)
    {
        registeredPlayerSO = playerSO;
        registeredPlayerBattleSO = playerBattleSO;
        registeredDefaultPlayerSO = defaultPlayerSO;

        CaptureDefaults();
        EnsureDefaultPlayerData();

        if (hasLoadedFromDisk)
        {
            ApplyLoadedPlayerData();
        }
    }

    public void CapturePlayerData()
    {
        if (registeredPlayerSO == null)
        {
            return;
        }

        if (currentPlayer == null)
        {
            currentPlayer = new PlayerSaveData();
        }

        // Safety net: whatever produced a negative value upstream (e.g. a passive-resource
        // catch-up calculation), never let it get written into the save file. Once a
        // negative value is persisted it reloads as negative forever, so clamp at the
        // point of capture rather than only where it's displayed.
        currentPlayer.woodResources = ClampResource(registeredPlayerSO.woodResources);
        currentPlayer.stoneResources = ClampResource(registeredPlayerSO.stoneResources);
        currentPlayer.farmResources = ClampResource(registeredPlayerSO.farmResources);
        currentPlayer.energyPoints = ClampResource(registeredPlayerSO.energyPoints);
        currentPlayer.researchPoints = ClampResource(registeredPlayerSO.researchPoints);
        currentPlayer.gems = ClampResource(registeredPlayerSO.gems);
        currentPlayer.coins = ClampResource(registeredPlayerSO.coins);
        currentPlayer.unlockedUnitKeys = new List<string>();

        if (registeredPlayerSO.unlockedUnits != null)
        {
            foreach (UnitSO unit in registeredPlayerSO.unlockedUnits)
            {
                if (unit == null)
                {
                    continue;
                }

                currentPlayer.unlockedUnitKeys.Add(GetUnitSaveKey(unit));
            }
        }

        // Trained troops live on PlayerBattleSO, not PlayerSO, and previously were never
        // written to the save file at all - they only survived as whatever state happened
        // to be serialized onto the PlayerBattleSO asset itself, which is not reliable
        // across separate application launches (see ApplyLoadedPlayerData for restore).
        currentPlayer.trainedUnitKeys = new List<string>();

        if (registeredPlayerBattleSO?.playerUnitStats != null)
        {
            foreach (UnitSO unit in registeredPlayerBattleSO.playerUnitStats)
            {
                if (unit == null)
                {
                    continue;
                }

                currentPlayer.trainedUnitKeys.Add(GetUnitSaveKey(unit));
            }
        }
    }

    public void ApplyLoadedPlayerData()
    {
        if (Application.isEditor)
        {
            return;
        }

        if (registeredPlayerSO == null || currentPlayer == null)
        {
            return;
        }

        registeredPlayerSO.woodResources = ClampResource(currentPlayer.woodResources);
        registeredPlayerSO.stoneResources = ClampResource(currentPlayer.stoneResources);
        registeredPlayerSO.farmResources = ClampResource(currentPlayer.farmResources);
        registeredPlayerSO.energyPoints = ClampResource(currentPlayer.energyPoints);
        registeredPlayerSO.researchPoints = ClampResource(currentPlayer.researchPoints);
        registeredPlayerSO.gems = ClampResource(currentPlayer.gems);
        registeredPlayerSO.coins = ClampResource(currentPlayer.coins);

        if (registeredPlayerSO.unlockedUnits == null)
        {
            registeredPlayerSO.unlockedUnits = new List<UnitSO>();
        }
        else
        {
            registeredPlayerSO.unlockedUnits.Clear();
        }

        if (currentPlayer.unlockedUnitKeys != null)
        {
            foreach (string unitKey in currentPlayer.unlockedUnitKeys)
            {
                if (TryResolveUnit(unitKey, out UnitSO unitSO) && !registeredPlayerSO.unlockedUnits.Contains(unitSO))
                {
                    registeredPlayerSO.unlockedUnits.Add(unitSO);
                }
            }
        }

        if (registeredPlayerBattleSO != null)
        {
            if (registeredPlayerBattleSO.playerUnitStats == null)
            {
                registeredPlayerBattleSO.playerUnitStats = new List<UnitSO>();
            }
            else
            {
                registeredPlayerBattleSO.playerUnitStats.Clear();
            }

            if (currentPlayer.trainedUnitKeys != null)
            {
                foreach (string unitKey in currentPlayer.trainedUnitKeys)
                {
                    if (TryResolveUnit(unitKey, out UnitSO unitSO))
                    {
                        registeredPlayerBattleSO.playerUnitStats.Add(unitSO);
                    }
                }
            }

            // playerUnits (parallel BuildingData list) is rebuilt to match playerUnitStats
            // by PlayerData.updateUnitList(), which is called again after this method runs.
            registeredPlayerBattleSO.playerUnits?.Clear();
        }
    }

    public void RegisterAvailableUnit(UnitSO unitSO)
    {
        if (unitSO == null)
        {
            return;
        }

        string unitKey = GetUnitSaveKey(unitSO);
        if (!unitRegistry.ContainsKey(unitKey))
        {
            unitRegistry.Add(unitKey, unitSO);
        }

        if (!string.IsNullOrWhiteSpace(unitSO.name) && !unitRegistry.ContainsKey(unitSO.name))
        {
            unitRegistry.Add(unitSO.name, unitSO);
        }
    }

    public void RestoreInto(BuildingSystem buildingSystem)
    {
        if (buildingSystem == null || buildingSystem.isBattleScene || !HasSavedKingdom)
        {
            return;
        }

        buildingSystem.RestoreKingdomState(currentKingdom);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        if (!HasSavedKingdom)
        {
            return;
        }

        StartCoroutine(RestoreKingdomAfterSceneLoad());
    }

    private System.Collections.IEnumerator RestoreKingdomAfterSceneLoad()
    {
        yield return null;

        BuildingSystem[] buildingSystems = FindObjectsByType<BuildingSystem>(FindObjectsSortMode.None);
        foreach (BuildingSystem buildingSystem in buildingSystems)
        {
            RestoreInto(buildingSystem);
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && ShouldUseDiskPersistence())
        {
            SaveCurrentKingdom();
        }
    }

    private void OnApplicationQuit()
    {
        if (ShouldUseDiskPersistence())
        {
            SaveCurrentKingdom();
        }
    }

    private void LoadFromDisk()
    {
        string path = GetSaveFilePath();
        if (!File.Exists(path))
        {
            // This is the ONLY case where we can be sure there's no real save yet.
            isFreshSave = true;
            EnsureDefaultPlayerData();
            hasLoadedFromDisk = true;
            return;
        }

        // A save file exists, so whatever values it contains (even all zeros) are real
        // player progress, not an uninitialized save. Never let EnsureDefaultPlayerData
        // second-guess this.
        isFreshSave = false;

        string json = File.ReadAllText(path);
        GameSaveData loadedSave = JsonUtility.FromJson<GameSaveData>(json);
        if (loadedSave != null)
        {
            currentPlayer = loadedSave.player ?? new PlayerSaveData();
            currentKingdom = loadedSave.kingdom ?? new SaveKingdomData();
            buildingSnapshots.Clear();
            if (currentKingdom.buildings != null)
            {
                buildingSnapshots.AddRange(currentKingdom.buildings);
            }
        }

        EnsureDefaultPlayerData();

        hasLoadedFromDisk = true;
    }

    private void CaptureDefaults()
    {
        if (registeredDefaultPlayerSO != null && playerDefaults.Count == 0)
        {
            playerDefaults.Add(CapturePlayerSnapshot(registeredDefaultPlayerSO));
        }
        else if (registeredPlayerSO != null && playerDefaults.Count == 0)
        {
            playerDefaults.Add(CapturePlayerSnapshot(registeredPlayerSO));
        }

        if (levelDefaults.Count == 0)
        {
            LevelSO[] levels = Resources.FindObjectsOfTypeAll<LevelSO>();
            foreach (LevelSO levelSO in levels)
            {
                if (levelSO == null)
                {
                    continue;
                }

                levelDefaults.Add(new LevelSnapshot
                {
                    levelSO = levelSO,
                    isUnlocked = levelSO.isUnlocked,
                    isCompleted = levelSO.isCompleted,
                    rewardClaimed = levelSO.rewardClaimed
                });
            }
        }

        if (unitDefaults.Count == 0)
        {
            UnitSO[] units = Resources.FindObjectsOfTypeAll<UnitSO>();
            foreach (UnitSO unitSO in units)
            {
                if (unitSO == null)
                {
                    continue;
                }

                unitDefaults.Add(new UnitSnapshot
                {
                    unitSO = unitSO,
                    level = unitSO.level,
                    health = unitSO.health,
                    damage = unitSO.damage,
                    attackRange = unitSO.attackRange,
                    mobility = unitSO.mobility,
                    movePoints = unitSO.movePoints,
                    attackPoints = unitSO.attackPoints,
                    unitCost = unitSO.unitCost,
                    harvestAmount = unitSO.harvestAmount
                });
            }
        }
    }

    private PlayerSaveData CapturePlayerSnapshot(PlayerSO source)
    {
        PlayerSaveData snapshot = new PlayerSaveData();

        if (source != null)
        {
            snapshot.woodResources = source.woodResources;
            snapshot.stoneResources = source.stoneResources;
            snapshot.farmResources = source.farmResources;
            snapshot.energyPoints = source.energyPoints;
            snapshot.researchPoints = source.researchPoints;
            snapshot.gems = source.gems;
            snapshot.coins = source.coins;
            snapshot.unlockedUnitKeys = new List<string>();

            if (source.unlockedUnits != null)
            {
                foreach (UnitSO unit in source.unlockedUnits)
                {
                    if (unit == null)
                    {
                        continue;
                    }

                    snapshot.unlockedUnitKeys.Add(GetUnitSaveKey(unit));
                }
            }
        }

        return snapshot;
    }

    private PlayerSaveData ClonePlayerSaveData(PlayerSaveData source)
    {
        if (source == null)
        {
            return new PlayerSaveData();
        }

        return new PlayerSaveData
        {
            woodResources = source.woodResources,
            stoneResources = source.stoneResources,
            farmResources = source.farmResources,
            energyPoints = source.energyPoints,
            researchPoints = source.researchPoints,
            gems = source.gems,
            coins = source.coins,
            unlockedUnitKeys = source.unlockedUnitKeys != null ? new List<string>(source.unlockedUnitKeys) : new List<string>(),
            trainedUnitKeys = source.trainedUnitKeys != null ? new List<string>(source.trainedUnitKeys) : new List<string>()
        };
    }

    private void EnsureDefaultPlayerData()
    {
        // Only fall back to defaults when we positively know this is a fresh save
        // (no file existed) or currentPlayer hasn't been created at all. Never use
        // "values happen to be zero" as a signal - a real player can legitimately be
        // at 0 across the board (e.g. right after spending everything on a troop).
        if (currentPlayer != null && !isFreshSave)
        {
            return;
        }

        PlayerSaveData defaultPlayerSnapshot = playerDefaults.Count > 0
            ? playerDefaults[0]
            : CapturePlayerSnapshot(registeredDefaultPlayerSO != null ? registeredDefaultPlayerSO : registeredPlayerSO);

        if (ShouldUseDefaultPlayerData(defaultPlayerSnapshot))
        {
            return;
        }

        currentPlayer = ClonePlayerSaveData(defaultPlayerSnapshot);

        // Defaults have now been applied once. From this point on this is "real" player
        // data (even though it currently equals the defaults), so later calls must not
        // clobber whatever the player earns/spends from here.
        isFreshSave = false;
    }

    private bool ShouldUseDefaultPlayerData(PlayerSaveData playerData)
    {
        if (playerData == null)
        {
            return true;
        }

        bool hasNoResources = playerData.woodResources == 0
            && playerData.stoneResources == 0
            && playerData.farmResources == 0
            && playerData.energyPoints == 0
            && playerData.researchPoints == 0
            && playerData.gems == 0
            && playerData.coins == 0;

        bool hasNoUnits = playerData.unlockedUnitKeys == null || playerData.unlockedUnitKeys.Count == 0;

        return hasNoResources && hasNoUnits;
    }

    private void ApplyCapturedPlayerSnapshot(PlayerSaveData snapshot)
    {
        if (registeredPlayerSO == null || snapshot == null)
        {
            return;
        }

        registeredPlayerSO.woodResources = ClampResource(snapshot.woodResources);
        registeredPlayerSO.stoneResources = ClampResource(snapshot.stoneResources);
        registeredPlayerSO.farmResources = ClampResource(snapshot.farmResources);
        registeredPlayerSO.energyPoints = ClampResource(snapshot.energyPoints);
        registeredPlayerSO.researchPoints = ClampResource(snapshot.researchPoints);
        registeredPlayerSO.gems = ClampResource(snapshot.gems);
        registeredPlayerSO.coins = ClampResource(snapshot.coins);

        if (registeredPlayerSO.unlockedUnits == null)
        {
            registeredPlayerSO.unlockedUnits = new List<UnitSO>();
        }
        else
        {
            registeredPlayerSO.unlockedUnits.Clear();
        }

        if (snapshot.unlockedUnitKeys != null)
        {
            foreach (string unitKey in snapshot.unlockedUnitKeys)
            {
                if (TryResolveUnit(unitKey, out UnitSO unitSO) && !registeredPlayerSO.unlockedUnits.Contains(unitSO))
                {
                    registeredPlayerSO.unlockedUnits.Add(unitSO);
                }
            }
        }
    }

    private void RestoreDefaults()
    {
        if (registeredPlayerSO != null && playerDefaults.Count > 0)
        {
            ApplyCapturedPlayerSnapshot(playerDefaults[0]);
        }

        foreach (LevelSnapshot snapshot in levelDefaults)
        {
            if (snapshot?.levelSO == null)
            {
                continue;
            }

            snapshot.levelSO.isUnlocked = snapshot.isUnlocked;
            snapshot.levelSO.isCompleted = snapshot.isCompleted;
            snapshot.levelSO.rewardClaimed = snapshot.rewardClaimed;
        }

        foreach (UnitSnapshot snapshot in unitDefaults)
        {
            if (snapshot?.unitSO == null)
            {
                continue;
            }

            snapshot.unitSO.level = snapshot.level;
            snapshot.unitSO.health = snapshot.health;
            snapshot.unitSO.damage = snapshot.damage;
            snapshot.unitSO.attackRange = snapshot.attackRange;
            snapshot.unitSO.mobility = snapshot.mobility;
            snapshot.unitSO.movePoints = snapshot.movePoints;
            snapshot.unitSO.attackPoints = snapshot.attackPoints;
            snapshot.unitSO.unitCost = snapshot.unitCost;
            snapshot.unitSO.harvestAmount = snapshot.harvestAmount;
        }

        if (registeredPlayerBattleSO != null)
        {
            registeredPlayerBattleSO.playerUnits?.Clear();
            registeredPlayerBattleSO.playerUnitStats?.Clear();
        }
    }

    private void SaveToDisk()
    {
        SyncCurrentKingdomFromSnapshots();

        GameSaveData saveData = new GameSaveData
        {
            player = currentPlayer ?? new PlayerSaveData(),
            kingdom = currentKingdom ?? new SaveKingdomData()
        };

        string path = GetSaveFilePath();
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(path, json);
    }

    private string GetSaveFilePath()
    {
        return Path.Combine(Application.persistentDataPath, SaveFileName);
    }

    private bool ShouldUseDiskPersistence()
    {
        return true; 
    }

    // Resources should never be negative. This is a safety net only - it stops a negative
    // value from being displayed or persisted to the save file, it does not address
    // whatever upstream system actually produced the negative value in the first place
    // (most likely the passive-resource building tick/upkeep system).
    private static int ClampResource(int value)
    {
        return Mathf.Max(0, value);
    }

    private string GetUnitSaveKey(UnitSO unitSO)
    {
        if (unitSO == null)
        {
            return string.Empty;
        }

        return string.IsNullOrWhiteSpace(unitSO.unitName) ? unitSO.name : unitSO.unitName;
    }

    private bool TryResolveUnit(string unitKey, out UnitSO unitSO)
    {
        unitSO = null;

        if (string.IsNullOrWhiteSpace(unitKey))
        {
            return false;
        }

        if (unitRegistry.TryGetValue(unitKey, out unitSO))
        {
            return unitSO != null;
        }

        UnitSO[] loadedUnits = Resources.FindObjectsOfTypeAll<UnitSO>();
        foreach (UnitSO loadedUnit in loadedUnits)
        {
            if (loadedUnit == null)
            {
                continue;
            }

            if (GetUnitSaveKey(loadedUnit) == unitKey || loadedUnit.name == unitKey)
            {
                unitSO = loadedUnit;
                return true;
            }
        }

        return false;
    }

    private BuildingSystem FindCurrentKingdomBuildingSystem()
    {
        BuildingSystem[] systems = FindObjectsByType<BuildingSystem>(FindObjectsSortMode.None);
        foreach (BuildingSystem system in systems)
        {
            if (system != null && !system.isBattleScene)
            {
                return system;
            }
        }

        return null;
    }

    private SavedBuildingData CreateBuildingSnapshot(Building building, List<Vector3> occupiedPositions = null)
    {
        BuildingModel buildingModel = building.GetComponentInChildren<BuildingModel>(true);
        PassiveResource passiveResource = building.GetComponentInChildren<PassiveResource>(true);

        List<Vector3> positions = occupiedPositions != null && occupiedPositions.Count > 0
            ? new List<Vector3>(occupiedPositions)
            : buildingModel != null ? buildingModel.GetAllBuildingPosition() : new List<Vector3>();

        return new SavedBuildingData
        {
            buildingKey = building.Name,
            worldPosition = building.transform.position,
            rootRotation = building.transform.eulerAngles.y,
            rotation = buildingModel != null ? buildingModel.Rotation : building.transform.eulerAngles.y,
            occupiedPositions = positions,
            passiveResource = passiveResource != null ? passiveResource.CaptureSaveData() : new PassiveResourceSaveData()
        };
    }

    private void RefreshPassiveResourceSnapshotsFromScene()
    {
        Building[] placedBuildings = FindObjectsByType<Building>(FindObjectsSortMode.None);
        foreach (Building building in placedBuildings)
        {
            if (building == null || !building.HasData)
            {
                continue;
            }

            PassiveResource passiveResource = building.GetComponentInChildren<PassiveResource>(true);
            if (passiveResource == null)
            {
                continue;
            }

            SavedBuildingData liveSnapshot = CreateBuildingSnapshot(building);
            int snapshotIndex = buildingSnapshots.FindIndex(existing => IsSameBuilding(existing, liveSnapshot));

            if (snapshotIndex >= 0)
            {
                buildingSnapshots[snapshotIndex].passiveResource = passiveResource.CaptureSaveData();
            }
            else
            {
                buildingSnapshots.Add(liveSnapshot);
            }
        }
    }

    private void SyncCurrentKingdomFromSnapshots()
    {
        currentKingdom = new SaveKingdomData
        {
            buildings = new List<SavedBuildingData>(buildingSnapshots)
        };
    }

    private static bool IsSameBuilding(SavedBuildingData existing, SavedBuildingData incoming)
    {
        if (existing == null || incoming == null)
        {
            return false;
        }

        return existing.buildingKey == incoming.buildingKey && ArePositionsClose(existing.worldPosition, incoming.worldPosition);
    }

    private static bool ArePositionsClose(Vector3 a, Vector3 b)
    {
        return Vector3.SqrMagnitude(a - b) < 0.0001f;
    }
}