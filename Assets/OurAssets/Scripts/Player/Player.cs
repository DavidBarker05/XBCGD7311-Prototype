using UnityEngine;
using UnityEngine.InputSystem;

public struct MouseInfo
{
    public Vector3 MouseScreenPosition;
    public bool bHitObject;
    public RaycastHit HitInfo;
}

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    [SerializeField]
    PlayerSettings m_PlayerSettings;
    [SerializeField]
    PlayerCharacter m_PlayerCharacter;
    [SerializeField]
    PipePlayerCharacter m_PipePlayerCharacter;
    [SerializeField]
    PlayerCamera m_PlayerCamera;
    [SerializeField]
    Camera m_Camera;

    PlayerInput m_PlayerInput;

    CharacterInput m_CharacterInput;
    PipeCharacterInput m_PipeCharacterInput;
    CameraInput m_CameraInput;

    MouseInfo m_PipeMouseInfo;

    bool m_bCursorHidden;

    void Awake()
    {
        m_PlayerInput = GetComponent<PlayerInput>();
        m_PlayerCharacter.Init(m_PlayerSettings.CharacterSettings);
        m_PipePlayerCharacter.Init();
        m_PlayerCamera.Init(m_PlayerSettings.CameraSettings, m_PlayerCharacter.CameraTarget);
        m_CharacterInput = new CharacterInput();
        m_PipeCharacterInput = new PipeCharacterInput();
        m_CameraInput = new CameraInput();
        m_PipeMouseInfo = new MouseInfo();
    }

    void Update()
    {
        switch (m_PlayerInput.currentActionMap.name)
        {
            case "Player":
                if (!m_bCursorHidden) HideCursor();
                m_PlayerCamera.UpdateRotation(ref m_CameraInput, Time.deltaTime);
                m_CharacterInput.Rotation = m_PlayerCamera.transform.rotation;
                m_PlayerCharacter.UpdatePosition(ref m_CharacterInput, Time.deltaTime);
                break;
            case "PipePlayer":
                if (m_bCursorHidden) ShowCursor();
                m_PipeMouseInfo.MouseScreenPosition = GetMousePositionOnScreen();
                GetMouseInfo(ref m_PipeMouseInfo, m_PipePlayerCharacter.HitLayer);
                m_PipeCharacterInput.PipeMouseInfo = m_PipeMouseInfo;
                m_PipePlayerCharacter.UpdatePipeCharacter(ref m_PipeCharacterInput);
                break;
            default:
                if (m_bCursorHidden) ShowCursor();
                break;
        }
    }

    void LateUpdate()
    {
        switch (m_PlayerInput.currentActionMap.name)
        {
            case "Player":
                m_PlayerCamera.UpdatePosition(m_PlayerCharacter.CameraTarget);
                break;
            case "PipePlayer":
                m_PlayerCamera.UpdatePosition(m_PipePlayerCharacter.CameraTarget);
                break;
            default:
                break;
        }
    }

    public void ChangeActionMap(string actionMap)
    {
        if (m_PlayerInput.currentActionMap.name == actionMap) return;
        m_PlayerInput.SwitchCurrentActionMap(actionMap);
        switch (actionMap)
        {
            case "Player":
                m_PlayerCamera.ChangeCameraTarget(m_PlayerCharacter.CameraTarget);
                break;
            case "PipePlayer":
                m_PlayerCamera.ChangeCameraTarget(m_PipePlayerCharacter.CameraTarget);
                break;
            default:
                break;
        }
    }

    #region Cursor Toggles
    public void ShowCursor()
    {
        m_bCursorHidden = false;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void HideCursor()
    {
        m_bCursorHidden = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    #endregion

    public Vector3 GetMousePositionOnScreen()
    {
        Vector3 pos = Mouse.current.position.value;
        pos.z = m_Camera.nearClipPlane;
        return pos;
    }

    public void GetMouseInfo(ref MouseInfo mouseInfo, LayerMask layerToHit, float maxDistance = 100f)
    {
        Ray ray = m_Camera.ScreenPointToRay(mouseInfo.MouseScreenPosition);
        mouseInfo.bHitObject = Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerToHit);
        if (mouseInfo.bHitObject) mouseInfo.HitInfo = hit;
    }

    #region Handle PlayerInput Events
    public void HandleMoveInput(InputAction.CallbackContext ctx) => m_CharacterInput.MovementInput = ctx.ReadValue<Vector2>();

    public void HandleLookInput(InputAction.CallbackContext ctx)
    {
        m_CameraInput.LookInput = ctx.ReadValue<Vector2>();
        m_CameraInput.LookDevice = ctx.control.device;
    }

    public void HandleJumpInput(InputAction.CallbackContext ctx) => m_CharacterInput.bJumpPressedThisFrame = ctx.action.WasPressedThisFrame();

    public void HandleClickInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started) m_PipeCharacterInput.bClickedThisFrame = true;
    }

    #region Control Scheme Change
    public InputDevice CurrentDevice { get; private set; }

    public void HandleControlsChange(PlayerInput input) => CurrentDevice = input.devices.Count > 0 ? input.devices[0] : null;
    #endregion
    #endregion
}
