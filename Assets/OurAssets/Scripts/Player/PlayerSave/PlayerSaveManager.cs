using System.IO;
using UnityEngine;

public static class PlayerSaveManager
{
    public static PlayerSaveData CurrentSaveData { get; private set; }

    static readonly string SaveFile = "save.dat";

    public static void CreateNewSave()
    {
        CurrentSaveData = new PlayerSaveData();
        SaveGame();
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
    }
}
