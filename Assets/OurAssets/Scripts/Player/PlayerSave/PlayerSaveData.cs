[System.Serializable]
public class PlayerSaveData
{
    public const int CurrentVersionNumber = 2; // Increase this when add new data

    public int SaveDataVersionNumber = CurrentVersionNumber;

    public int DayNumber = 0; // 0 = Tutorial
    public int DaySeed = 0; // Save the seed for the current day so that same generation on restart

    public int Money = 0;

    public bool HasTVLicence = false;
    public int BiggerWallBreakHoleUpgradeLevel = 0;
    public int FasterSprintUpgradeLevel = 0;
    public bool HasEarnMoreMoneyUpgrade = false;
}

[System.Serializable]
public class PlayerSaveDataV0
{
    public int DayNumber = 0;
    public int DaySeed = 0;
    public int Money = 0;

    public PlayerSaveData ConvertToSaveData() => new PlayerSaveData()
    {
        DayNumber = DayNumber,
        DaySeed = DaySeed,
        Money = Money
    };
}

[System.Serializable]
public class PlayerSaveDataV1
{
    public int SaveDataVersionNumber = 1;

    public int DayNumber = 0;
    public int DaySeed = 0;

    public int Money = 0;

    public int BiggerWallBreakHoleUpgradeLevel = 0;
    public int FasterSprintUpgradeLevel = 0;
    public bool HasEarnMoreMoneyUpgrade = false;

    public PlayerSaveData ConvertToSaveData() => new PlayerSaveData()
    {
        DayNumber = DayNumber,
        DaySeed = DaySeed,
        Money = Money,
        BiggerWallBreakHoleUpgradeLevel = BiggerWallBreakHoleUpgradeLevel,
        FasterSprintUpgradeLevel = FasterSprintUpgradeLevel,
        HasEarnMoreMoneyUpgrade = HasEarnMoreMoneyUpgrade
    };
}
