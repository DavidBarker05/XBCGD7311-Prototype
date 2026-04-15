using UnityEngine;
using Util.SystemUtils;
using Util.UnityUtils;

public class FirstPersonPlayerCharacterInitData : IPlayerCharacterInitData
{
    public CharacterSettings CharacterSettings { get; set; }
    public InteractSettings InteractSettings { get; set; }
}

public class FirstPersonPlayerCharacterUpdateData : IPlayerCharacterUpdateData
{
    public float DeltaTime { get; set; }
    public Quaternion CameraRotation { get; set; }
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

    CharacterSettings m_CharacterSettings;
    InteractSettings m_InteractSettings;
    CharacterController m_CC;

    bool m_bIsGrounded;
    bool m_bBumpedHead;

    Vector3 m_Velocity;
    bool m_bIsJumping;
    bool m_bIsFalling;

    float m_CurrentJumpBufferTimer;
    float m_CurrentCoyoteTimer;

    float m_MovementSpeed;

	public override bool HasBeenInitialised { get; protected set; }

    public override bool MouseVisible => false;
    public override bool DoCameraRotation => true;
    public override bool UseMouseScreenPosition => false;

    void Awake() => m_CC = GetComponent<CharacterController>();

    public override void Init(IPlayerCharacterInitData playerCharacterInitData)
    {
		FirstPersonPlayerCharacterInitData initData = Sys.AssertType<FirstPersonPlayerCharacterInitData>(playerCharacterInitData, nameof(playerCharacterInitData));
        m_CharacterSettings = initData.CharacterSettings;
        m_InteractSettings = initData.InteractSettings;
		HasBeenInitialised = true;
    }

    public override void UpdateCharacter(ref IPlayerCharacterUpdateData playerCharacterUpdateData)
    {
		Sys.Assert(HasBeenInitialised, "FirstPersonPlayerCharacter hasn't been initialised");
		FirstPersonPlayerCharacterUpdateData input = Sys.AssertType<FirstPersonPlayerCharacterUpdateData>(playerCharacterUpdateData, nameof(playerCharacterUpdateData));
        HandleMovement(ref input);
        HandleInteraction(ref input);
    }

    #region Movement
    void HandleMovement(ref FirstPersonPlayerCharacterUpdateData input)
    {
        UpdateRotation(input.CameraRotation);
        CollisionChecks();
        UpdateTimers(input.DeltaTime);
        UpdateMovementSpeed(input.SprintPressedThisFrame);
        UpdateHorizontalVelocity(input.MovementInput);
        JumpChecks(input.JumpPressedThisFrame);
        UpdateVerticalVelocity(input.DeltaTime);
        m_CC.Move(m_Velocity * input.DeltaTime);
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
        bool bIsSprinting = ((m_bIsGrounded && !m_bIsFalling) || m_CharacterSettings.CanSprintInAir) && bSprintPressedThisFrame;
        m_MovementSpeed = bIsSprinting ? m_CharacterSettings.SprintSpeed : m_CharacterSettings.MovementSpeed;
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
    }
    #endregion
    #endregion Movement

    #region Interaction
    void HandleInteraction(ref FirstPersonPlayerCharacterUpdateData input)
    {
        if (input.PressedInteract)
        {
            Vector3 direction = input.CameraRotation * Vector3.forward; // Rotate forward vector by camera rotation to get camera's forward vector
            DoInteraction(direction);
        }
        input.PressedInteract = false;
    }

    void DoInteraction(Vector3 direction)
    {
        if (Physics.Raycast(
            origin: CameraTarget.position,
            direction: direction,
            hitInfo: out RaycastHit hit,
            maxDistance: m_InteractSettings.InteractionDistance,
            layerMask: m_InteractSettings.InteractableLayer,
            queryTriggerInteraction: QueryTriggerInteraction.Collide))
        {
            Interactable interactable = hit.GetComponent<Interactable>();
            if (interactable != null) interactable.Interact();
        }
    }
    #endregion Interaction
}
