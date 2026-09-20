using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSettings", menuName = "Player/Character Settings")]
public class CharacterSettings : ScriptableObject
{
    [field: SerializeField]
    public bool CanSprintInAir { get; private set; } = true;
    [field: SerializeField, Min(1f)]
    public float MovementSpeed { get; private set; } = 4.5f;
    [field: SerializeField, Min(1f)]
    public float SprintSpeed { get; private set; } = 10f;
    [field: SerializeField, Min(1f)]
    public float SprintSpeedUpgrade1 { get; private set; } = 11f;
    [field: SerializeField, Min(1f)]
    public float SprintSpeedUpgrade2 { get; private set; } = 12.5f;
    [field: SerializeField, Min(1f)]
    public float SprintSpeedUpgrade3 { get; private set; } = 14f;
    [field: SerializeField, Min(1f)]
    public float StepsPerSecond { get; private set; } = 2f;
    [field: SerializeField, Min(1f)]
    public float SprintStepsPerSecond { get; private set; } = 4f;
    [field: SerializeField, Range(0f, 180f)]
    public float VerticalFieldOfView { get; private set; } = 60f;
    [field: SerializeField, Range(0f, 180f)]
    public float SprintVerticalFieldOfView { get; private set; } = 75f;
    [field: SerializeField, Min(0f)]
    public float FieldOfViewTransitionDuration { get; private set; } = 0.25f;
    [field: SerializeField, Min(0f)]
    public float JumpHeight { get; private set; } = 1f;
    [field: SerializeField, Min(0f)]
    public float TimeToReachJumpHeight { get; private set; } = 0.25f;
    [field: SerializeField, Range(0f, 1f)]
    public float JumpBuffer { get; private set; } = 0.125f;
    [field: SerializeField, Range(0f, 1f)]
    public float CoyoteTime { get; private set; } = 0.1f;

    public float Gravity { get; private set; }
    public float PosGravity { get; private set; }
    public float InitialJumpVelocity { get; private set; }

    void OnValidate() => CalculateValues();

    void OnEnable() => CalculateValues();

    void CalculateValues()
    {
        Gravity = (-2f * JumpHeight) / (TimeToReachJumpHeight * TimeToReachJumpHeight);
        PosGravity = Mathf.Abs(Gravity);
        InitialJumpVelocity = PosGravity * TimeToReachJumpHeight;
    }
}
