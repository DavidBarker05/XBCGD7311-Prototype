using UnityEngine;

public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 MovementInput;
    public bool bJumpPressedThisFrame;
}

[RequireComponent(typeof(CharacterController))]
public class PlayerCharacter : MonoBehaviour
{
    [field: SerializeField]
    public Transform CameraTarget { get; private set; }

    const float EPSILON = 0.05f;
    const float SQR_EPSILON = EPSILON * EPSILON;

    CharacterSettings m_CharacterSettings;
    CharacterController m_CC;

    bool m_bIsGrounded;
    bool m_bBumpedHead;

    Vector3 m_Velocity;
    bool m_bIsJumping;
    bool m_bIsFalling;

    float m_CurrentJumpBufferTimer;
    float m_CurrentCoyoteTimer;

    void Awake() => m_CC = GetComponent<CharacterController>();

    public void Init(CharacterSettings characterSettings) => m_CharacterSettings = characterSettings;

    public void UpdatePosition(CharacterInput input, float deltaTime)
    {
        UpdateRotation(input.Rotation);
        CollisionChecks();
        UpdateTimers(deltaTime);
        UpdateHorizontalVelocity(input.MovementInput);
        JumpChecks(input.bJumpPressedThisFrame);
        UpdateVerticalVelocity(deltaTime);
        m_CC.Move(m_Velocity * deltaTime);
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

    void UpdateHorizontalVelocity(Vector2 movementInput)
    {
        float xIn = movementInput.x;
        float zIn = movementInput.y;
        Vector3 hIn = Vector3.ClampMagnitude(xIn * transform.right + zIn * transform.forward, 1f);
        hIn = hIn.sqrMagnitude > SQR_EPSILON ? hIn : Vector3.zero;
        m_Velocity.x = hIn.x * m_CharacterSettings.MovementSpeed;
        m_Velocity.z = hIn.z * m_CharacterSettings.MovementSpeed;
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
}
