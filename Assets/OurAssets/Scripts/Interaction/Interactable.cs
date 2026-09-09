using UnityEngine;

public struct InteractionStatus
{
    public bool EndInteraction;
    public object[] OutArguments;
}

public abstract class Interactable : MonoBehaviour
{
    public abstract InteractionStatus Interact(params object[] inputParameters);
}
