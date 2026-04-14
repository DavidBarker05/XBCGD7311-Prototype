using UnityEngine;

[CreateAssetMenu(fileName = "InteractSettings", menuName = "Player/Interact Settings")]
public class InteractSettings : ScriptableObject
{
    [field: SerializeField]
    public float InteractionDistance { get; private set; } = 2f;
    [field: SerializeField]
    public LayerMask InteractableLayer { get; private set; }
}
