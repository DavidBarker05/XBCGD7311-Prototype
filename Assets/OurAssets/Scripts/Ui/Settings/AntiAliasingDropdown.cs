using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AntiAliasingDropdown : MonoBehaviour
{
    [SerializeField]
    TMP_Dropdown m_ModeDropdown;
    [SerializeField]
    TMP_Dropdown m_QualityDropdown;
    [SerializeField]
    GameObject m_QualityDropdownContainer;

    void Awake()
    {
        m_ModeDropdown.ClearOptions();
        m_ModeDropdown.AddOptions(new List<string>(GameUserSettingsManager.AntiAliasingModes));
    }

    void OnEnable()
    {
        m_ModeDropdown.onValueChanged.RemoveAllListeners();
        m_QualityDropdown.onValueChanged.RemoveAllListeners();
        string current = GameUserSettingsManager.Instance?.AntiAliasing ?? GameUserSettingsManager.AntiAliasingTypes[0];
        (string mode, string quality) = GameUserSettingsManager.SplitAntiAliasingType(current);
        int modeIndex = Mathf.Max(Array.IndexOf(GameUserSettingsManager.AntiAliasingModes, mode), 0);
        m_ModeDropdown.SetValueWithoutNotify(modeIndex);
        RefreshQualityDropdown(mode, quality);
        m_ModeDropdown.onValueChanged.AddListener(OnModeChanged);
        m_QualityDropdown.onValueChanged.AddListener(OnQualityChanged);
    }

    void OnModeChanged(int index)
    {
        string mode = GameUserSettingsManager.AntiAliasingModes[index];
        string[] qualities = GameUserSettingsManager.AntiAliasingQualities[mode];
        string quality = qualities.Length > 0 ? qualities[0] : null;
        RefreshQualityDropdown(mode, quality);
        Apply(mode, quality);
    }

    void OnQualityChanged(int index)
    {
        string mode = GameUserSettingsManager.AntiAliasingModes[m_ModeDropdown.value];
        string[] qualities = GameUserSettingsManager.AntiAliasingQualities[mode];
        Apply(mode, qualities[index]);
    }

    void RefreshQualityDropdown(string mode, string currentQuality)
    {
        string[] qualities = GameUserSettingsManager.AntiAliasingQualities[mode];
        bool bHasQualities = qualities.Length > 0;
        (m_QualityDropdownContainer ? m_QualityDropdownContainer : m_QualityDropdown.gameObject).SetActive(bHasQualities);
        m_QualityDropdown.ClearOptions();
        if (!bHasQualities) return;
        m_QualityDropdown.AddOptions(new List<string>(qualities));
        int qualityIndex = Mathf.Max(Array.IndexOf(qualities, currentQuality), 0);
        m_QualityDropdown.SetValueWithoutNotify(qualityIndex);
    }

    void Apply(string mode, string quality)
    {
        if (GameUserSettingsManager.Instance) GameUserSettingsManager.Instance.AntiAliasing = GameUserSettingsManager.ComposeAntiAliasingType(mode, quality);
    }
}
