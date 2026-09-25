using UnityEngine;

public static class MinigameMoneyReward
{
    const float RandomMultiplierMin = 0.9f;
    const float RandomMultiplierMax = 1.1f;

    public static int Calculate(float baseMoney, float extraMultiplier = 1f)
    {
        float upgradeMultiplier = PlayerUpgradeSystem.IsPurchased(PlayerUpgrade.EarnMoreMoney, 1) ? 80f / 60f : 1f;
        float randomMultiplier = Random.Range(RandomMultiplierMin, RandomMultiplierMax);
        return Mathf.RoundToInt(baseMoney * extraMultiplier * randomMultiplier * upgradeMultiplier);
    }

    public static void Award(float baseMoney, float extraMultiplier = 1f)
    {
        if (PlayerSaveManager.CurrentSaveData == null) return;
        CurrencyManager.Instance?.ReceiveMoney(Calculate(baseMoney, extraMultiplier));
    }
}
