[System.Serializable]
public enum PlayerUpgrade
{
    BiggerWallBreakHole,
    FasterSprint,
    EarnMoreMoney
}

public static class PlayerUpgrades
{
    public static string GetDisplayName(PlayerUpgrade upgrade) => upgrade switch
    {
        PlayerUpgrade.BiggerWallBreakHole => "Bigger Sledge Hammer",
        PlayerUpgrade.FasterSprint => "New Kicks",
        PlayerUpgrade.EarnMoreMoney => "Sweet Talker",
        _ => throw new System.NotImplementedException($"Unknown upgrade \"{upgrade}\"")
    };

    public static int GetMaxLevel(PlayerUpgrade upgrade) => upgrade switch
    {
        PlayerUpgrade.BiggerWallBreakHole => 3,
        PlayerUpgrade.FasterSprint => 3,
        PlayerUpgrade.EarnMoreMoney => 1,
        _ => throw new System.NotImplementedException($"Unknown upgrade \"{upgrade}\"")
    };

    public static int GetUpgradeCost(PlayerUpgrade upgrade, int level) => upgrade switch
    {
        PlayerUpgrade.BiggerWallBreakHole => level switch
        {
            1 => 0, // TODO: Implement cost
            2 => 0, // TODO: Implement cost
            3 => 0, // TODO: Implement cost
            _ => throw new System.ArgumentException($"{level} is not a valid level for BiggerWallBreakHole")
        },
        PlayerUpgrade.FasterSprint => level switch
        {
            1 => 0, // TODO: Implement cost
            2 => 0, // TODO: Implement cost
            3 => 0, // TODO: Implement cost
            _ => throw new System.ArgumentException($"{level} is not a valid level for FasterSprint")
        },
        PlayerUpgrade.EarnMoreMoney => level == 1 ? 0 : throw new System.ArgumentException($"{level} is not a valid level for EarnMoreMoney"), // TODO: Implement cost
        _ => throw new System.NotImplementedException($"Unknown upgrade \"{upgrade}\"")
    };
}
