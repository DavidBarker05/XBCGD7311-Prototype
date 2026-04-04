using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler), typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField, Min(1f)]
    float movementSpeed = 4.5f;
    [SerializeField, Min(0f)]
    float jumpHeight = 1f;
    [SerializeField, Min(0f)]
    float timeToReachJumpHeight = 0.25f;
    [SerializeField, Range(0f, 1f)]
    float jumpBuffer = 0.125f;
    [SerializeField, Range(0f, 1f)]
    float coyoteTime = 0.1f;

    const float EPSILON = 0.05f;
    const float SQR_EPSILON = EPSILON * EPSILON;

    PlayerInputHandler pih;
    CharacterController cc;

    float gravity;
    float PosGravity => Mathf.Abs(gravity);
    float NegGravity => -PosGravity;
    float initalJumpVelocity;

    bool isGrounded;
    bool hasBumpedHead;

    bool isJumping;

    Vector3 velocity;

    float currentJumpBuffer;
    float currentCoyoteTime;

    void OnValidate() => CalculateValues();

    void Awake()
    {
        pih = GetComponent<PlayerInputHandler>();
        cc = GetComponent<CharacterController>();
        CalculateValues();
    }

    void Update()
    {
        CheckCollisions();
        HandleTimers();
        UpdateVelocity();
        cc.Move(velocity * Time.deltaTime);
    }

    void CalculateValues()
    {
        gravity = (-2f * jumpHeight) / (timeToReachJumpHeight * timeToReachJumpHeight);
        initalJumpVelocity = PosGravity * timeToReachJumpHeight;
    }

    #region Collision Checks
    void GroundCheck() => isGrounded = cc.isGrounded; // Simple rn, but can always do sphere casts if we need more complex stuff

    void BumpedHeadCheck() => hasBumpedHead = cc.collisionFlags.HasFlag(CollisionFlags.Above); // Simple rn, but can always do sphere casts if we need more complex stuff

    void CheckCollisions()
    {
        GroundCheck();
        BumpedHeadCheck();
    }
    #endregion

    void HandleTimers()
    {
        if (isGrounded) currentCoyoteTime = coyoteTime;
        else
        {
            currentJumpBuffer -= Time.deltaTime;
            currentCoyoteTime -= Time.deltaTime;
        }
    }

    #region Movement
    #region Horizontal Movement
    void UpdateHoriziontalVelocity()
    {
        float xIn = pih.MovementInput.x;
        float zIn = pih.MovementInput.y;
        Vector3 hIn = Vector3.ClampMagnitude(xIn * transform.right + zIn * transform.forward, 1f);
        hIn = hIn.sqrMagnitude > SQR_EPSILON ? hIn : Vector3.zero;
        velocity.x = hIn.x * movementSpeed;
        velocity.z = hIn.z * movementSpeed;
    }
    #endregion

    #region Vertical Movement
    #region Jumping
    void JumpChecks()
    {
        if (pih.JumpWasPressedThisFrame) currentJumpBuffer = jumpBuffer;
        if (currentJumpBuffer > 0f && !isJumping && (isGrounded || coyoteTime > 0f)) InitiateJump();
    }

    void InitiateJump()
    {
        isJumping = true;
        currentJumpBuffer = 0f;
        velocity.y = initalJumpVelocity;
    }
    #endregion

    void HandleVerticalMovementOnGround()
    {
        JumpChecks();
        if (velocity.y < 0f)
        {
            isJumping = false;
            velocity.y = -1f;
        }
    }

    void HandleGravity()
    {
        if (hasBumpedHead && velocity.y > 0f) velocity.y = 0f;
        velocity.y += NegGravity * Time.deltaTime;
    }

    void UpdateVerticalVelocity()
    {
        if (isGrounded) HandleVerticalMovementOnGround();
        else HandleGravity();
    }
    #endregion

    void UpdateVelocity()
    {
        UpdateHoriziontalVelocity();
        UpdateVerticalVelocity();
    }
    #endregion
}
