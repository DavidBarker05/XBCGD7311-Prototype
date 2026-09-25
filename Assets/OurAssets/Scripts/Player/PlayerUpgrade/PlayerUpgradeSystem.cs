public static class PlayerUpgradeSystem
{
    public static event System.Action<PlayerUpgrade, int> OnUpgradePurchased;

    public static int GetLevel(PlayerUpgrade upgrade) => upgrade switch
    {
        PlayerUpgrade.BiggerWallBreakHole => PlayerSaveManager.CurrentSaveData.BiggerWallBreakHoleUpgradeLevel,
        PlayerUpgrade.FasterSprint => PlayerSaveManager.CurrentSaveData.FasterSprintUpgradeLevel,
        PlayerUpgrade.EarnMoreMoney => PlayerSaveManager.CurrentSaveData.HasEarnMoreMoneyUpgrade ? 1 : 0,
        _ => throw new System.NotImplementedException($"Unknown upgrade \"{upgrade}\"")
    };

    static void SetLevel(PlayerUpgrade upgrade, int level)
    {
        switch (upgrade)
        {
            case PlayerUpgrade.BiggerWallBreakHole: PlayerSaveManager.CurrentSaveData.BiggerWallBreakHoleUpgradeLevel = level; break;
            case PlayerUpgrade.FasterSprint: PlayerSaveManager.CurrentSaveData.FasterSprintUpgradeLevel = level; break;
            case PlayerUpgrade.EarnMoreMoney: PlayerSaveManager.CurrentSaveData.HasEarnMoreMoneyUpgrade = level >= 1; break;
            default: throw new System.NotImplementedException($"Unknown upgrade \"{upgrade}\"");
        }
    }

    public static bool IsMaxLevel(PlayerUpgrade upgrade) => GetLevel(upgrade) >= PlayerUpgrades.GetMaxLevel(upgrade);

    public static bool IsPurchased(PlayerUpgrade upgrade, int level) => GetLevel(upgrade) >= level;

    public static bool CanPurchase(PlayerUpgrade upgrade, int level)
    {
        if (PlayerSaveManager.CurrentSaveData == null) return false;
        if (level < 1 || level > PlayerUpgrades.GetMaxLevel(upgrade)) return false;
        if (level != GetLevel(upgrade) + 1) return false; // Not the next level up from what's already owned
        return (CurrencyManager.Instance?.GetMoney ?? 0) >= PlayerUpgrades.GetUpgradeCost(upgrade, level);
    }

    public static bool TryPurchase(PlayerUpgrade upgrade, int level)
    {
        if (!CanPurchase(upgrade, level)) return false;
        CurrencyManager.Instance?.LoseMoney(PlayerUpgrades.GetUpgradeCost(upgrade, level));
        SetLevel(upgrade, level);
        OnUpgradePurchased?.Invoke(upgrade, level);
        return true;
    }
}
