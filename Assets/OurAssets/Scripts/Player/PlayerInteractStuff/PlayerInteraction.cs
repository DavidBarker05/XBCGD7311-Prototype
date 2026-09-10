using UnityEngine;

public interface IPlayerInteractionInitData
{
    public Camera Camera { get; set; }
    public Camera HoldCamera { get; set; }
    public Camera HoldClipCamera { get; set; }
    public Transform HoldPosTransform { get; set; }
}

public interface IPlayerInteractionUpdateData
{
    public float DeltaTime { get; set; }
}

public abstract class PlayerInteraction : MonoBehaviour
{
    [SerializeField]
    protected InteractSettings m_InteractSettings;

    public abstract bool HasBeenInitialised { get; protected set; }

    protected Interactable m_CurrentInteraction;

    public abstract void Init(IPlayerInteractionInitData playerInteractionInitData);
    public abstract void UpdateInteraction(ref IPlayerInteractionUpdateData playerInteractionUpdateData);

    protected abstract Interactable CheckForInteraction();

    public abstract void Interact();
}
