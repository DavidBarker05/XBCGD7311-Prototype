using UnityEngine;
using Util.SystemUtils;

public class FirstPersonPlayerCharacterInitData : IPlayerCharacterInitData
{
    public CharacterSettings CharacterSettings { get; set; }
    public Player Player { get; set; }
    public PauseCharacter PauseCharacter { get; set; }

    public Camera Camera { get; set; }
}

public class FirstPersonPlayerCharacterUpdateData : IPlayerCharacterUpdateData
{
    public float DeltaTime { get; set; }
    public MouseInfo MouseInfo { get; set; }

    public Vector2 MovementInput { get; set; }
    public bool JumpPressedThisFrame { get; set; }
    public bool SprintPressedThisFrame { get; set; }
    public bool PressedInteract { get; set; }
}

[RequireComponent(typeof(CharacterController))]
public class FirstPersonPlayerCharacter : PlayerCharacter
{
    static readonly float s_Epsilon = 0.05f;
    static readonly float s_SqrEpsilon = s_Epsilon * s_Epsilon;

    [SerializeField]
    FirstPersonPlayerCharacterInteraction m_FirstPersonPlayerCharacterInteraction;
    [SerializeField]
    AudioSource m_FootstepSource;

    Camera m_Camera;
    CharacterSettings m_CharacterSettings;
    CharacterController m_CC;

    bool m_bIsGrounded;
    bool m_bBumpedHead;

    Vector3 m_Velocity;
    bool m_bIsJumping;
    bool m_bIsFalling;

    float m_CurrentJumpBufferTimer;
    float m_CurrentCoyoteTimer;

    bool m_bIsSprinting;
    float m_MovementSpeed;
    float m_CurrentFootstepTime;

    public override bool HasBeenInitialised { get; protected set; }

    public override string ActionMap => "Player";
    public override bool MouseVisible => false;
    public override bool DoCameraRotation => true;
    public override bool UseMouseScreenPosition => false;

    PauseCharacter m_PauseCharacter;

    void Awake() => m_CC = GetComponent<CharacterController>();

    public override void Init(IPlayerCharacterInitData playerCharacterInitData)
    {
        FirstPersonPlayerCharacterInitData initData = Sys.AssertType<FirstPersonPlayerCharacterInitData>(playerCharacterInitData, nameof(playerCharacterInitData));
        m_CharacterSettings = initData.CharacterSettings;
        m_PauseCharacter = initData.PauseCharacter;
        m_Camera = initData.Camera;
        m_FirstPersonPlayerCharacterInteraction.Init(new FirstPersonPlayerCharacterInteractionInitData()
        {
            Camera = m_Camera,
            Player = initData.Player,
            FirstPersonPlayerCharacter = this
        });
        HasBeenInitialised = true;
    }

    public override void UpdateCharacter(ref IPlayerCharacterUpdateData playerCharacterUpdateData)
    {
        Sys.Assert(HasBeenInitialised, "FirstPersonPlayerCharacter hasn't been initialised");
        FirstPersonPlayerCharacterUpdateData updateData = Sys.AssertType<FirstPersonPlayerCharacterUpdateData>(playerCharacterUpdateData, nameof(playerCharacterUpdateData));
        HandleMovement(ref updateData);
        HandleInteraction(ref updateData);
        IPlayerInteractionUpdateData interactionUpdateData = new FirstPersonPlayerCharacterInteractionUpdateData() { DeltaTime = updateData.DeltaTime };
        m_FirstPersonPlayerCharacterInteraction.UpdateInteraction(ref interactionUpdateData);
    }

    public override void OnPausePressed()
    {
        Sys.Assert(HasBeenInitialised, "FirstPersonPlayerCharacter hasn't been initialised");
        m_PauseCharacter.PauseGame(this);
    }

    #region Movement
    void HandleMovement(ref FirstPersonPlayerCharacterUpdateData updateData)
    {
        UpdateRotation(m_Camera.transform.rotation);
        CollisionChecks();
        UpdateTimers(updateData.DeltaTime);
        UpdateMovementSpeed(updateData.SprintPressedThisFrame);
        UpdateHorizontalVelocity(updateData.MovementInput);
        JumpChecks(updateData.JumpPressedThisFrame);
        UpdateVerticalVelocity(updateData.DeltaTime);
        HandleFootstep(updateData.DeltaTime);
        HandleCameraFOV(updateData.DeltaTime);
        m_CC.Move(m_Velocity * updateData.DeltaTime);
    }

    void UpdateRotation(Quaternion rotation)
    {
        Vector3 forward = Vector3.ProjectOnPlane(rotation * Vector3.forward, Vector3.up).normalized;
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    #region Collision Checks
    void GroundCheck() => m_bIsGrounded = m_CC.isGrounded; // Simple rn, but can always do sphere casts if we need more complex stuff

    void BumpedHeadCheck() => m_bBumpedHead = m_CC.collisionFlags.HasFlag(CollisionFlags.Above); // Simple rn, but can always do sphere casts if we need more complex stuff

    void CollisionChecks()
    {
        GroundCheck();
        BumpedHeadCheck();
    }
    #endregion

    void UpdateTimers(float deltaTime)
    {
        m_CurrentJumpBufferTimer -= deltaTime;
        if (!m_bIsGrounded) m_CurrentCoyoteTimer -= deltaTime;
        else m_CurrentCoyoteTimer = 0f;
    }

    void UpdateMovementSpeed(bool bSprintPressedThisFrame)
    {
        m_bIsSprinting = ((m_bIsGrounded && !m_bIsFalling) || m_CharacterSettings.CanSprintInAir) && bSprintPressedThisFrame;
        m_MovementSpeed = m_bIsSprinting ? PlayerUpgradeSystem.GetLevel(PlayerUpgrade.FasterSprint) switch
        {
            1 => m_CharacterSettings.SprintSpeedUpgrade1,
            2 => m_CharacterSettings.SprintSpeedUpgrade2,
            3 => m_CharacterSettings.SprintSpeedUpgrade3,
            _ => m_CharacterSettings.SprintSpeed
        } : m_CharacterSettings.MovementSpeed;
    }

    void UpdateHorizontalVelocity(Vector2 movementInput)
    {
        float xIn = movementInput.x;
        float zIn = movementInput.y;
        Vector3 hIn = Vector3.ClampMagnitude(xIn * transform.right + zIn * transform.forward, 1f);
        hIn = hIn.sqrMagnitude > s_SqrEpsilon ? hIn : Vector3.zero;
        m_Velocity.x = hIn.x * m_MovementSpeed;
        m_Velocity.z = hIn.z * m_MovementSpeed;
    }

    #region Jumping
    void JumpChecks(bool bJumpPressedThisFrame)
    {
        if (bJumpPressedThisFrame) m_CurrentJumpBufferTimer = m_CharacterSettings.JumpBuffer;
        if (m_CurrentJumpBufferTimer > 0f && !m_bIsJumping && (m_bIsGrounded || m_CurrentCoyoteTimer > 0f)) InitiateJump();
        if ((m_bIsJumping || m_bIsFalling) && m_bIsGrounded && m_Velocity.y <= 0f)
        {
            m_bIsJumping = false;
            m_bIsFalling = false;
            m_Velocity.y = 0f;
        }
    }

    void InitiateJump()
    {
        m_bIsJumping = true;
        m_CurrentJumpBufferTimer = 0f;
        m_Velocity.y = m_CharacterSettings.InitialJumpVelocity;
    }
    #endregion

    #region Update Vertical Velocity
    void UpdateVerticalVelocityWhileJumping(float deltaTime)
    {
        if (m_bBumpedHead && m_Velocity.y > 0) m_Velocity.y = 0f;
        m_Velocity.y -= m_CharacterSettings.PosGravity * deltaTime;
        m_bIsFalling = m_Velocity.y < 0f;
    }

    void UpdateVerticalVelocityWhileFalling(float deltaTime)
    {
        m_bIsFalling = true;
        m_Velocity.y -= m_CharacterSettings.PosGravity * deltaTime;
    }

    void UpdateVerticalVelocity(float deltaTime)
    {
        if (m_bIsJumping) UpdateVerticalVelocityWhileJumping(deltaTime);
        else if (!m_bIsGrounded) UpdateVerticalVelocityWhileFalling(deltaTime);
        else m_Velocity.y = -1f;
    }
    #endregion

    void HandleFootstep(float deltaTime)
    {
        if (!m_bIsGrounded)
        {
            m_CurrentFootstepTime = 0f;
            return;
        }
        float hSqrSpeed = new Vector2(m_Velocity.x, m_Velocity.z).sqrMagnitude;
        if (hSqrSpeed < s_SqrEpsilon) return;
        if (m_CurrentFootstepTime == 0f) m_FootstepSource.Play();
        m_CurrentFootstepTime += deltaTime;
        float stepsPerSecond = m_bIsSprinting ? m_CharacterSettings.SprintStepsPerSecond : m_CharacterSettings.StepsPerSecond;
        if (m_CurrentFootstepTime < 1f / stepsPerSecond) return;
        m_FootstepSource.Play();
        m_CurrentFootstepTime = 0f;
    }

    void HandleCameraFOV(float deltaTime)
    {
        float targetFOV = m_bIsSprinting ? m_CharacterSettings.SprintVerticalFieldOfView : m_CharacterSettings.VerticalFieldOfView;
        float fovRange = Mathf.Abs(m_CharacterSettings.SprintVerticalFieldOfView - m_CharacterSettings.VerticalFieldOfView);
        float transitionSpeed = m_CharacterSettings.FieldOfViewTransitionDuration > 0f ? fovRange / m_CharacterSettings.FieldOfViewTransitionDuration : float.MaxValue;
        m_Camera.fieldOfView = Mathf.MoveTowards(m_Camera.fieldOfView, targetFOV, transitionSpeed * deltaTime);
    }
    #endregion Movement

    #region Interaction
    void HandleInteraction(ref FirstPersonPlayerCharacterUpdateData updateData)
    {
        if (updateData.PressedInteract) m_FirstPersonPlayerCharacterInteraction.Interact();
        updateData.PressedInteract = false;
    }
    #endregion Interaction
}
