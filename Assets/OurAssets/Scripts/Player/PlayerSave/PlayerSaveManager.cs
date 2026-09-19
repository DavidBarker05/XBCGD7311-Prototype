using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PlayerSaveManager
{
    public static PlayerSaveData CurrentSaveData { get; private set; }

    public static bool DoesSaveExist { get; private set; }

    static readonly string SaveFile = "save.dat";

    static readonly int s_MainMenuSceneIndex = 0;
    static readonly int s_TutorialSceneIndex = 1;
    static readonly int s_MainGameSceneIndex = 2;

    const int TutorialRandomSeed = 761218;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        LoadSave();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int sceneIndex = scene.buildIndex;
        if (sceneIndex == s_MainMenuSceneIndex) DoesSaveExist = CurrentSaveData != null;
        else if (sceneIndex == s_TutorialSceneIndex)
        {
            DoesSaveExist = true;
            CreateNewSave();
            UseSeedForCurrentDay();
        }
        else if (sceneIndex == s_MainGameSceneIndex)
        {
            DoesSaveExist = true;
            if (CurrentSaveData == null)
            {
                CurrentSaveData = new PlayerSaveData()
                {
                    DayNumber = 1,
                    DaySeed = GenerateEntropySeed()
                };
                SaveGame();
            }
            UseSeedForCurrentDay();
        }
    }

    public static void CreateNewSave()
    {
        CurrentSaveData = new PlayerSaveData()
        {
            DaySeed = GenerateEntropySeed()
        };
        SaveGame();
    }

    static int GenerateEntropySeed() => Random.Range(int.MinValue, int.MaxValue) ^ unchecked((int)System.DateTime.UtcNow.Ticks);

    public static void SaveGame()
    {
        string json = JsonUtility.ToJson(CurrentSaveData, false);
        string encryptedJson = EncryptionUtility.EncryptString(json);
        string saveLocation = Path.Combine(Application.persistentDataPath, SaveFile);
        File.WriteAllText(saveLocation, encryptedJson);
    }

    // Only used to peek the version number before picking which type to actually deserialise the save as
    [System.Serializable]
    class SaveDataVersionPeek
    {
        public int SaveDataVersionNumber = 0;
    }

    public static void LoadSave()
    {
        string saveLocation = Path.Combine(Application.persistentDataPath, SaveFile);
        if (!File.Exists(saveLocation)) return;
        string encryptedJson = File.ReadAllText(saveLocation);
        string json = EncryptionUtility.DecryptString(encryptedJson);

        int version = JsonUtility.FromJson<SaveDataVersionPeek>(json).SaveDataVersionNumber;
        CurrentSaveData = ConvertToSaveData(json, version);
        if (version < PlayerSaveData.CurrentVersionNumber) SaveGame();
    }

    static PlayerSaveData ConvertToSaveData(string json, int version) => version switch
    {
        0 => JsonUtility.FromJson<PlayerSaveDataV0>(json).ConvertToSaveData(),
        PlayerSaveData.CurrentVersionNumber => JsonUtility.FromJson<PlayerSaveData>(json),
        _ => UnknownVersionFallback(json, version)
    };

    static PlayerSaveData UnknownVersionFallback(string json, int version)
    {
#if UNITY_EDITOR
        Debug.LogWarning($"Save data reports version {version}, which is newer than this build understands (current is {PlayerSaveData.CurrentVersionNumber}). Loading it as the current version and hoping for the best.");
#endif
        return JsonUtility.FromJson<PlayerSaveData>(json);
    }

    public static void GenerateRandomSeed() => CurrentSaveData.DaySeed = GenerateEntropySeed();

    public static void UseSeedForCurrentDay()
    {
        int seed = CurrentSaveData.DayNumber == 0 ? TutorialRandomSeed : CurrentSaveData.DaySeed;
        Random.InitState(seed);
    }
}
