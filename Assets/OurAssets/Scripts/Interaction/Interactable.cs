using UnityEngine;

public struct InteractionStatus
{
    public bool EndInteraction;
    public object[] OutArguments;
}

public abstract class Interactable : MonoBehaviour
{
    [field: SerializeField]
    public Transform InteractPromptTransform { get; private set; }

    public virtual bool CanInteractWith => true;

    public abstract InteractionStatus Interact(params object[] inputParameters);
}
