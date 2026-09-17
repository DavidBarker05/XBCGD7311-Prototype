using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HSensSlider : MonoBehaviour
{
    [SerializeField]
    HSensInputField m_SensInput;

    Slider m_Slider;
    public Slider Slider
    {
        get
        {
            m_Slider ??= GetComponent<Slider>();
            return m_Slider;
        }
    }

    void OnEnable()
    {
        Slider.onValueChanged.RemoveAllListeners();
        m_SensInput.InputField.onSubmit.RemoveAllListeners();
        float sens = GameUserSettingsManager.Instance?.HorizontalSensitivityMultiplier ?? 1f;
        Slider.value = Mathf.RoundToInt((sens - GameUserSettingsManager.MIN_SENS_MULT_UI) / (GameUserSettingsManager.MAX_SENS_MULT_UI - GameUserSettingsManager.MIN_SENS_MULT_UI) * (Slider.maxValue - Slider.minValue));
        m_SensInput.InputField.text = sens.ToString("F1");
        Slider.onValueChanged.AddListener(ChangeSens);
        m_SensInput.InputField.onSubmit.AddListener(m_SensInput.ChangeSens);
    }

    public void ChangeSens(float value)
    {
        if (!GameUserSettingsManager.Instance) return;
        m_SensInput.InputField.onSubmit.RemoveAllListeners();
        float sens = GameUserSettingsManager.MIN_SENS_MULT_UI + value / (Slider.maxValue - Slider.minValue) * (GameUserSettingsManager.MAX_SENS_MULT_UI - GameUserSettingsManager.MIN_SENS_MULT_UI);
        m_SensInput.InputField.text = sens.ToString("F1");
        GameUserSettingsManager.Instance.HorizontalSensitivityMultiplier = sens;
        m_SensInput.InputField.onSubmit.AddListener(m_SensInput.ChangeSens);
    }
}
