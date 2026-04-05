using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    [SerializeField]
    PlayerSettings m_PlayerSettings;
    [SerializeField]
    PlayerCharacter m_PlayerCharacter;
    [SerializeField]
    PlayerCamera m_PlayerCamera;

    CharacterInput m_CharacterInput;
    CameraInput m_CameraInput;

    void Awake()
    {
        m_PlayerCharacter.Init(m_PlayerSettings.CharacterSettings);
        m_PlayerCamera.Init(m_PlayerSettings.CameraSettings, m_PlayerCharacter.transform, m_PlayerCharacter.CameraTarget);
        m_CameraInput = new CameraInput();
        m_CharacterInput = new CharacterInput();
        HideCursor();
    }

    void Update()
    {
        m_PlayerCamera.UpdateRotation(m_CameraInput, Time.deltaTime);
        m_PlayerCharacter.UpdatePosition(m_CharacterInput, Time.deltaTime);
    }

    void LateUpdate() => m_PlayerCamera.UpdatePosition(m_PlayerCharacter.CameraTarget);

    #region Cursor Toggles
    public void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    #endregion

    #region Handle PlayerInput Events
    public void HandleMoveInput(InputAction.CallbackContext ctx) => m_CharacterInput.MovementInput = ctx.ReadValue<Vector2>();

    public void HandleLookInput(InputAction.CallbackContext ctx)
    {
        m_CameraInput.LookInput = ctx.ReadValue<Vector2>();
        m_CameraInput.LookDevice = ctx.control.device;
    }

    public void HandleJumpInput(InputAction.CallbackContext ctx) => m_CharacterInput.bJumpPressedThisFrame = ctx.action.WasPressedThisFrame();

    #region Control Scheme Change
    public InputDevice CurrentDevice { get; private set; }

    public void HandleControlsChange(PlayerInput input) => CurrentDevice = input.devices.Count > 0 ? input.devices[0] : null;
    #endregion
    #endregion
}
