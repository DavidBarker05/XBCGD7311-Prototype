using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    [SerializeField]
    PlayerSettings m_PlayerSettings;
    [SerializeField]
    PlayerCharacter m_StartingPlayerCharacter;
    [SerializeField]
    MenuCharacter m_MenuCharacter;
    [SerializeField]
    PauseCharacter m_PauseCharacter;
    [SerializeField]
    PlayerCamera m_PlayerCamera;
    [SerializeField]
    Camera m_Camera;

    PlayerInput m_PlayerInput;

    PlayerCharacter m_PlayerCharacter;
    IPlayerCharacterUpdateData m_PlayerCharacterUpdateData;
    CameraInput m_CameraInput;
    MouseInfo m_MouseInfo;

    void Awake()
    {
        m_PlayerInput = GetComponent<PlayerInput>();
        m_PlayerCamera.Init(m_PlayerSettings.CameraSettings);
        m_MenuCharacter.Init(new MenuCharacterInitData() { Player = this });
        m_PauseCharacter.Init(new PauseCharacterInitData() { Player = this });
        ChangeCharacter(m_StartingPlayerCharacter);
        m_CameraInput = new CameraInput();
        m_MouseInfo = new MouseInfo();
    }

    void Update()
    {
        if (!m_PlayerCharacter || !m_PlayerCamera) return;
        SetCursorVisibility(m_PlayerCharacter.MouseVisible);
        m_PlayerCharacterUpdateData.DeltaTime = Time.deltaTime;
        if (m_PlayerCharacter.DoCameraRotation)
        {
            m_PlayerCamera.UpdateRotation(ref m_CameraInput, Time.deltaTime);
            m_PlayerCharacterUpdateData.CameraRotation = m_PlayerCamera.transform.rotation;
        }
        if (m_PlayerCharacter.UseMouseScreenPosition)
        {
            m_MouseInfo.MouseScreenPosition = GetMousePositionOnScreen();
            GetMouseInfo(ref m_MouseInfo, m_PlayerCharacter.MouseHitLayer, m_PlayerCharacter.MouseHitDistance);
            m_PlayerCharacterUpdateData.MouseInfo = m_MouseInfo;
        }
        m_PlayerCharacter.UpdateCharacter(ref m_PlayerCharacterUpdateData);
    }

    void LateUpdate() => m_PlayerCamera.UpdatePosition(m_PlayerCharacter.CameraTarget);

    #region Change Character
    public void ChangeCharacter(PlayerCharacter playerCharacter)
    {
        if (!playerCharacter) return;
        m_PlayerCharacter = playerCharacter;
        if (!m_PlayerCharacter.HasBeenInitialised) m_PlayerCharacter.Init(PlayerCharacterInitData);
        m_PlayerCharacterUpdateData = PlayerCharacterUpdateData;
        if (!string.IsNullOrWhiteSpace(m_PlayerCharacter.ActionMap)) m_PlayerInput.SwitchCurrentActionMap(m_PlayerCharacter.ActionMap);
        m_PlayerCamera.ChangeCameraTarget(m_PlayerCharacter.CameraTarget);
        SetCursorVisibility(m_PlayerCharacter.MouseVisible);
    }

    IPlayerCharacterInitData PlayerCharacterInitData => m_PlayerCharacter switch
    {
        FirstPersonPlayerCharacter => new FirstPersonPlayerCharacterInitData()
        {
            CharacterSettings = m_PlayerSettings.CharacterSettings,
            InteractSettings = m_PlayerSettings.InteractSettings,
            Player = this,
            PauseCharacter = m_PauseCharacter
        },
        PipePlayerCharacter => new PipePlayerCharacterInitData() { PauseCharacter = m_PauseCharacter },
        WirePlayerCharacter => new WirePlayerCharacterInitData() { PauseCharacter = m_PauseCharacter },
        WallKnockPlayerCharacter => new WallKnockPlayerCharacterInitData() { PauseCharacter = m_PauseCharacter },
        QTEPlayerCharacter => new QTEPlayerCharacterInitData(),
        MenuCharacter => new MenuCharacterInitData() { Player = this },
        PauseCharacter => new PauseCharacterInitData() { Player = this },
        _ => null
    };

    IPlayerCharacterUpdateData PlayerCharacterUpdateData => m_PlayerCharacter switch
    {
        FirstPersonPlayerCharacter => new FirstPersonPlayerCharacterUpdateData(),
        PipePlayerCharacter => new PipePlayerCharacterUpdateData(),
        WirePlayerCharacter => new WirePlayerCharacterUpdateData(),
        WallKnockPlayerCharacter => new WallKnockPlayerCharacterUpdateData(),
        QTEPlayerCharacter => new QTEPlayerCharacterUpdateData(),
        MenuCharacter => new MenuCharacterUpdateData(),
        PauseCharacter => new PauseCharacterUpdateData(),
        _ => null
    };
    #endregion Change Character

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

    public void SetCursorVisibility(bool bVisible)
    {
        if (bVisible) ShowCursor();
        else HideCursor();
    }
    #endregion Cursor Toggles

    #region Mouse Info
    public Vector3 GetMousePositionOnScreen()
    {
        Vector3 pos = Mouse.current.position.value;
        pos.z = m_Camera.nearClipPlane;
        return pos;
    }

    public void GetMouseInfo(ref MouseInfo mouseInfo, LayerMask layerToHit, float maxDistance = 100f)
    {
        mouseInfo.IsMouseOverUI = EventSystem.current?.IsPointerOverGameObject() ?? false; // Don't forget to make panels not raycast targets
        Ray ray = m_Camera.ScreenPointToRay(mouseInfo.MouseScreenPosition);
        mouseInfo.DidHitObject = Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerToHit);
        if (mouseInfo.DidHitObject) mouseInfo.HitInfo = hit;
    }
    #endregion Mouse Info

    #region Handle PlayerInput Events

    delegate void SetDataValueFunc<T>(T _) where T : class, IPlayerCharacterUpdateData;

    void SetDataValue<T>(SetDataValueFunc<T> dataChangeFunction) where T : class, IPlayerCharacterUpdateData
    {
        if (m_PlayerCharacterUpdateData is T t) dataChangeFunction(t);
    }

    public void HandleMoveInput(InputAction.CallbackContext ctx)
    {
        SetDataValue<FirstPersonPlayerCharacterUpdateData>(updateData => updateData.MovementInput = ctx.ReadValue<Vector2>());
    }

    public void HandleLookInput(InputAction.CallbackContext ctx)
    {
        if (!m_PlayerCharacter.DoCameraRotation) return;
        m_CameraInput.LookInput = ctx.ReadValue<Vector2>();
        m_CameraInput.LookDevice = ctx.control.device;
    }

    public void HandleSprintInput(InputAction.CallbackContext ctx)
    {
        SetDataValue<FirstPersonPlayerCharacterUpdateData>(updateData => updateData.SprintPressedThisFrame = ctx.action.WasPressedThisFrame());
    }

    public void HandleInteractInput(InputAction.CallbackContext ctx)
    {
        SetDataValue<FirstPersonPlayerCharacterUpdateData>(updateData => { if (ctx.started) updateData.PressedInteract = true; });
    }

    public void HandleJumpInput(InputAction.CallbackContext ctx)
    {
        SetDataValue<FirstPersonPlayerCharacterUpdateData>(updateData => updateData.JumpPressedThisFrame = ctx.action.WasPressedThisFrame());
    }

    public void HandleLeftClickInput(InputAction.CallbackContext ctx)
    {
        SetDataValue<PipePlayerCharacterUpdateData>(updateData => updateData.LeftClickedThisFrame |= ctx.started);
        SetDataValue<WirePlayerCharacterUpdateData>(updateData => updateData.ClickedThisFrame = ctx.action.WasPressedThisFrame());
        SetDataValue<WallKnockPlayerCharacterUpdateData>(updateData => updateData.LeftClickedThisFrame |= ctx.started);
    }

    public void HandleRightClickInput(InputAction.CallbackContext ctx)
    {
        SetDataValue<WallKnockPlayerCharacterUpdateData>(updateData => updateData.RightClickedThisFrame |= ctx.started);
        SetDataValue<PipePlayerCharacterUpdateData>(updateData => updateData.RightClickedThisFrame |= ctx.started);
    }

    public void HandleDoQTEInput(InputAction.CallbackContext ctx)
    {
        SetDataValue<QTEPlayerCharacterUpdateData>(updateData => updateData.DidQTEInput |= ctx.started);
    }

    public void HandlePauseInput(InputAction.CallbackContext ctx) => m_PlayerCharacter.OnPausePressed();

    public void HandleRotateLeft(InputAction.CallbackContext ctx)
    {
        SetDataValue<PipePlayerCharacterUpdateData>(updateData => updateData.PressedLeftRotateThisFrame |= ctx.started);
    }

    public void HandleRotateRight(InputAction.CallbackContext ctx)
    {
        SetDataValue<PipePlayerCharacterUpdateData>(updateData => updateData.PressedRightRotateThisFrame |= ctx.started);
    }

    #region Control Scheme Change
    public InputDevice CurrentDevice { get; private set; }

    public void HandleControlsChange(PlayerInput input) => CurrentDevice = input.devices.Count > 0 ? input.devices[0] : null;
    #endregion Control Scheme Change
    #endregion Handle PlayerInput Events
}
