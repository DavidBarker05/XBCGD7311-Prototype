using UnityEngine;

[CreateAssetMenu(fileName = "CameraSettings", menuName = "Player/Camera Settings")]
public class CameraSettings : ScriptableObject
{
    [field: SerializeField, Min(0f)]
    public float HorizontalSensitivity { get; private set; } = 0.2f;
    [field: SerializeField, Min(0f)]
    public float VerticalSensitivity { get; private set; } = 0.225f;
    [field: SerializeField, Range(-90f, 0f)]
    public float MinVerticalAngle { get; private set; } = -80f;
    [field: SerializeField, Range(0f, 90f)]
    public float MaxVerticalAngle { get; private set; } = 80f;
}
