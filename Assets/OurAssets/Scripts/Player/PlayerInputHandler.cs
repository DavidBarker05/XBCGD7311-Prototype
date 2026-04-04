using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    #region Movement Input
    public Vector2 MovementInput { get; private set; }

    public void HandleMoveInput(InputAction.CallbackContext ctx) => MovementInput = ctx.ReadValue<Vector2>();
    #endregion

    #region Look Input
    public Vector2 LookInput { get; private set; }
    public InputDevice LookDevice { get; private set; }

    public void HandleLookInput(InputAction.CallbackContext ctx)
    {
        LookInput = ctx.ReadValue<Vector2>();
        LookDevice = ctx.control.device;
    }
    #endregion

    #region Jump Input
    public bool JumpWasPressedThisFrame { get; private set; }
    public bool JumpWasReleasedThisFrame { get; private set; }

    public void HandleJumpInput(InputAction.CallbackContext ctx)
    {
        JumpWasPressedThisFrame = ctx.started || ctx.performed;
        JumpWasReleasedThisFrame = ctx.canceled;
    }
    #endregion

    #region Control Scheme Change
    public InputDevice CurrentDevice { get; private set; }

    public void HandleControlsChange(PlayerInput input) => CurrentDevice = input.devices.Count > 0 ? input.devices[0] : null;
    #endregion
}
