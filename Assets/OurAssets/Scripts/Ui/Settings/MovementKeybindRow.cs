using System;
using System.Collections.Generic;
using UnityEngine;
public class MovementKeybindRow : MonoBehaviour
{
    [SerializeField]
    string m_PartName = "up";
    [SerializeField]
    KeybindButton m_MainKeybindButton;
    [SerializeField]
    KeybindButton m_SecondaryKeybindButton;

    void OnEnable()
    {
        if (!GameUserSettingsManager.Instance) return;
        List<(int BindingIndex, string PartName, string CurrentDisplayString)> parts = GameUserSettingsManager.Instance.GetCompositePartBindings("Player", "Move");
        int foundCount = 0;
        foreach ((int BindingIndex, string PartName, string CurrentDisplayString) part in parts)
        {
            if (!string.Equals(part.PartName, m_PartName, StringComparison.OrdinalIgnoreCase)) continue;
            KeybindButton target = foundCount switch
            {
                0 => m_MainKeybindButton,
                1 => m_SecondaryKeybindButton,
                _ => null
            };
            target?.Configure("Player", "Move", part.BindingIndex);
            ++foundCount;
        }

#if UNITY_EDITOR
        if (foundCount < 2) Debug.LogWarning($"WARNING: MovementKeybindRow \"{m_PartName}\" only found {foundCount} matching part-binding(s) on Move - expected a main and a secondary");
#endif
    }
}
