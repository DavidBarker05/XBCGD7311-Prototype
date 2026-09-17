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
}
