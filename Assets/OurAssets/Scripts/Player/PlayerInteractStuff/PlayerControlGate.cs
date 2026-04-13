using UnityEngine;

public class PlayerControlGate : MonoBehaviour
{
    [Header("Scripts To Toggle")]
    [SerializeField] private MonoBehaviour[] movementScripts;
    [SerializeField] private MonoBehaviour[] cameraScripts;

    [Header("Optional Objects To Toggle")]
    [SerializeField] private GameObject[] gameplayInputObjects;

    public void SetGameplayEnabled(bool enabled)
    {
        ToggleBehaviours(movementScripts, enabled);
        ToggleBehaviours(cameraScripts, enabled);
        ToggleObjects(gameplayInputObjects, enabled);
        SetCursorForState(enabled);
    }

    private static void ToggleBehaviours(MonoBehaviour[] scripts, bool enabled)
    {
        if (scripts == null)
        {
            return;
        }

        for (int i = 0; i < scripts.Length; i++)
        {
            if (scripts[i] != null)
            {
                scripts[i].enabled = enabled;
            }
        }
    }

    private static void ToggleObjects(GameObject[] objects, bool enabled)
    {
        if (objects == null)
        {
            return;
        }

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
            {
                objects[i].SetActive(enabled);
            }
        }
    }

    private static void SetCursorForState(bool gameplayEnabled)
    {
        Cursor.visible = !gameplayEnabled;
        Cursor.lockState = gameplayEnabled ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
