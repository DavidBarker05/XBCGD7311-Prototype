using UnityEngine;
using UnityEngine.UI;
using Util.SystemUtils;

[RequireComponent(typeof(Button))]
public class QuitButton : MonoBehaviour
{
    void Awake() => GetComponent<Button>().onClick.AddListener(() => Sys.Exit(0));
}
