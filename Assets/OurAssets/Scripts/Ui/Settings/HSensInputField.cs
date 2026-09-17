using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class HSensInputField : MonoBehaviour
{
    [SerializeField]
    HSensSlider m_SensSlider;

    TMP_InputField m_InputField;
    public TMP_InputField InputField
    {
        get
        {
            m_InputField ??= GetComponent<TMP_InputField>();
            return m_InputField;
        }
    }

    public void ChangeSens(string input)
    {
        if (!GameUserSettingsManager.Instance) return;
        InputField.onSubmit.RemoveAllListeners();
        m_SensSlider.Slider.onValueChanged.RemoveAllListeners();
        float sens = Mathf.Round(Mathf.Clamp(float.Parse(input), GameUserSettingsManager.MIN_SENS_MULT_UI, GameUserSettingsManager.MAX_SENS_MULT_UI) * 10f) / 10f;
        InputField.text = sens.ToString("F1");
        m_SensSlider.Slider.value = Mathf.RoundToInt((sens - GameUserSettingsManager.MIN_SENS_MULT_UI) / (GameUserSettingsManager.MAX_SENS_MULT_UI - GameUserSettingsManager.MIN_SENS_MULT_UI) * (m_SensSlider.Slider.maxValue - m_SensSlider.Slider.minValue));
        GameUserSettingsManager.Instance.HorizontalSensitivityMultiplier = sens;
        InputField.onSubmit.AddListener(ChangeSens);
        m_SensSlider.Slider.onValueChanged.AddListener(m_SensSlider.ChangeSens);
    }
}
