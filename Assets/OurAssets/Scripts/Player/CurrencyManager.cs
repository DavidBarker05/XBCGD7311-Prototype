using UnityEngine;
using UnityEngine.Events;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    public int GetMoney => PlayerSaveManager.CurrentSaveData?.Money ?? 0;

    public UnityEvent OnCurrencyChanged;

    void Awake()
    {
        if (Instance && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void ReceiveMoney(int amount)
    {
        if (amount <= 0 || PlayerSaveManager.CurrentSaveData == null) return;
        PlayerSaveManager.CurrentSaveData.Money += amount;
        OnCurrencyChanged?.Invoke();
    }

    public void LoseMoney(int amount)
    {
        if (amount <= 0 || PlayerSaveManager.CurrentSaveData == null) return;
        PlayerSaveManager.CurrentSaveData.Money -= amount;
        OnCurrencyChanged?.Invoke();
    }
}
