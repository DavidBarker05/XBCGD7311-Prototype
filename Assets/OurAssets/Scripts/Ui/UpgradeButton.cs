using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UpgradeButton : MonoBehaviour
{
    [SerializeField]
    PlayerUpgrade m_Upgrade;
    [SerializeField, Min(1)]
    int m_Level = 1;
    [SerializeField]
    TextMeshProUGUI m_Label;

    Button m_Button;

    void Awake()
    {
        m_Button = GetComponent<Button>();
        if (!m_Label) m_Label = GetComponentInChildren<TextMeshProUGUI>();
        m_Button.onClick.AddListener(Purchase);
    }

    void OnEnable()
    {
        PlayerUpgradeSystem.OnUpgradePurchased += HandleUpgradePurchased;
        Refresh();
    }

    void OnDisable() => PlayerUpgradeSystem.OnUpgradePurchased -= HandleUpgradePurchased;

    void HandleUpgradePurchased(PlayerUpgrade upgrade, int level) => Refresh();

    void Purchase() => PlayerUpgradeSystem.TryPurchase(m_Upgrade, m_Level);

    void Refresh()
    {
        bool bPurchased = PlayerUpgradeSystem.IsPurchased(m_Upgrade, m_Level);
        m_Button.interactable = !bPurchased && PlayerUpgradeSystem.CanPurchase(m_Upgrade, m_Level);
        if (!m_Label) return;
        string priceOrStatus = bPurchased ? "Purchased" : PlayerUpgrades.GetUpgradeCost(m_Upgrade, m_Level).ToString();
        m_Label.text = $"{PlayerUpgrades.GetDisplayName(m_Upgrade)}\n{priceOrStatus}";
    }
}
