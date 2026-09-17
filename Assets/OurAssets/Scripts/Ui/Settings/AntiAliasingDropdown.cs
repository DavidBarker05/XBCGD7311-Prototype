using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Dropdown))]
public class AntiAliasingDropdown : MonoBehaviour
{
    TMP_Dropdown m_Dropdown;

    void Awake()
    {
        m_Dropdown = GetComponent<TMP_Dropdown>();
        m_Dropdown.ClearOptions();
        m_Dropdown.AddOptions(new List<string>(GameUserSettingsManager.AntiAliasingTypes));
    }

    void OnEnable()
    {
        m_Dropdown.onValueChanged.RemoveAllListeners();
        string current = GameUserSettingsManager.Instance?.AntiAliasing ?? GameUserSettingsManager.AntiAliasingTypes[0];
        int index = Array.IndexOf(GameUserSettingsManager.AntiAliasingTypes, current);
        m_Dropdown.value = Mathf.Max(index, 0);
        m_Dropdown.onValueChanged.AddListener(ChangeAntiAliasing);
    }

    void ChangeAntiAliasing(int index)
    {
        if (GameUserSettingsManager.Instance) GameUserSettingsManager.Instance.AntiAliasing = GameUserSettingsManager.AntiAliasingTypes[index];
    }
}
