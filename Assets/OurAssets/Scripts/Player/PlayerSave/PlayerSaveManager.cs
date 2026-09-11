using System.IO;
using UnityEngine;

public static class PlayerSaveManager
{
    public static PlayerSaveData CurrentSaveData { get; private set; }

    static readonly string SaveFile = "save.dat";

    const int TutorialRandomSeed = 761218;

    // TEMPORARY: Remove when all systems in place
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        LoadSave();
        if (CurrentSaveData == null) CreateNewSave();
    }

    public static void CreateNewSave()
    {
        CurrentSaveData = new PlayerSaveData()
        {
            DaySeed = Random.Range(int.MinValue, int.MaxValue)
        };
        SaveGame();
        SeedRandomForCurrentDay();
    }

    public static void SaveGame()
    {
        string json = JsonUtility.ToJson(CurrentSaveData, false);
        string encryptedJson = EncryptionUtility.EncryptString(json);
        string saveLocation = Path.Combine(Application.persistentDataPath, SaveFile);
        File.WriteAllText(saveLocation, encryptedJson);
    }

    public static void LoadSave()
    {
        string saveLocation = Path.Combine(Application.persistentDataPath, SaveFile);
        if (!File.Exists(saveLocation)) return;
        string encryptedJson = File.ReadAllText(saveLocation);
        string json = EncryptionUtility.DecryptString(encryptedJson);
        CurrentSaveData = JsonUtility.FromJson<PlayerSaveData>(json);
        SeedRandomForCurrentDay();
    }

    public static void SeedRandomForCurrentDay()
    {
        int seed = CurrentSaveData.DayNumber == 0 ? TutorialRandomSeed : CurrentSaveData.DaySeed;
        Random.InitState(seed);
    }
}
