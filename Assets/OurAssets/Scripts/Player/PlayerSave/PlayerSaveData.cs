[System.Serializable]
public class PlayerSaveData
{
    public int DayNumber = 0; // 0 = Tutorial
    public int DaySeed = 0; // Save the seed for the current day so that same generation on restart
    public int Money = 0;
    // TODO: Add purchased upgrades
}
