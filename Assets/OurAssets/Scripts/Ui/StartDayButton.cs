using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class StartDayButton : MonoBehaviour
{
    void Awake() => GetComponent<Button>().onClick.AddListener(() => GameManager.Instance.StartDay());
}
