using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SaveSettingsButton : MonoBehaviour
{
    void Awake() => GetComponent<Button>().onClick.AddListener(() => GameUserSettingsManager.Instance.SaveSettings());
}
