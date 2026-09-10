using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Holdable : Interactable
{
    [SerializeField]
    float maxReleaseVelocity = 10f;
    [SerializeField]
    float groundDistance = 0.1f;
    [SerializeField]
    LayerMask groundLayer;
    [SerializeField, Range(2, 10), Tooltip("The maximum number of \"ground\" colliders checked and stored in memory")]
    int maxGroundColliders = 6;
    [SerializeField, Range(5, 15), Tooltip("The maximum number of colliders checked and stored in memory when handling clipping")]
    int maxClippingColliders = 10;
    [SerializeField, Range(1, 10), Tooltip("The maximum number of attempts the object will try to prevent itself from clipping before just respawning")]
    int maxUnclippingAttempts = 5;

    Rigidbody rb;
    Collider _collider;
    Transform startingParentTransform;
    Collider[] groundColliders;
    Collider[] clippingColliders;
    Transform[] childTransforms;
    LayerMask playerMask;
    Quaternion startRot;
    Vector3 startPos;
    Vector3 lastPos;
    Vector3 releaseVel;
    int startLayer;
    bool held = false;
    bool isClipping = false;
    bool isStartLayerAlsoGround;

    [HideInInspector]
    public Vector3 StartPos => startPos;
    [HideInInspector]
    public Quaternion StartRot => startRot;
    public bool IsGrounded
    {
        get
        {
            System.Collections.Generic.HashSet<Holdable> checkedHoldables = new System.Collections.Generic.HashSet<Holdable>();
            return IsGroundedInternal(checkedHoldables);
        }
    }

    protected void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        startingParentTransform = transform.parent;
        groundColliders = new Collider[maxGroundColliders];
        clippingColliders = new Collider[maxClippingColliders];
        childTransforms = GetComponentsInChildren<Transform>();
        playerMask = LayerMask.GetMask("Player");
        startRot = transform.rotation;
        startPos = transform.position;
        startLayer = gameObject.layer;
        isStartLayerAlsoGround = ((1 << startLayer) & groundLayer) != 0;
    }

    void FixedUpdate()
    {
        if (!held) return; // Don't do calculations if not being held
        if (lastPos == transform.position) return; // Don't do calculations if hasn't moved
        releaseVel = Vector3.ClampMagnitude((transform.position - lastPos) / Time.fixedDeltaTime, maxReleaseVelocity);
        lastPos = transform.position;
    }

    void OnCollisionEnter(Collision collision) { if (collision.gameObject.CompareTag("OutOfBounds")) RespawnObject(); }

    public override InteractionStatus Interact(params object[] parameters)
    {
        if (parameters.Length != 4)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"WARNING: Holdable objects needs 4 parameters. Received {parameters.Length} parameters");
#endif
            return new InteractionStatus() { EndInteraction = true }; // Drop object
        }
        if (parameters[0] is Transform holdPos && parameters[1] is LayerMask holdLayer && parameters[2] is Collider playerCollider && parameters[3] is Transform camTransform)
        {
            held = !held; // If held then release, if not held then hold
            transform.parent = held ? holdPos : null; // If now held attach to the hold position transform otherwise detach it
            Physics.IgnoreCollision(_collider, playerCollider, held);
            if (held)
            {
                rb.isKinematic = true;
                rb.detectCollisions = false; // Make sure we can't push other interactables through floor
                if (transform.localPosition != Vector3.zero) transform.localPosition = Vector3.zero; // Centre to the hold position
                if (lastPos != transform.position) lastPos = transform.position; // Update last position for release physics
            }
            else
            {
                rb.detectCollisions = true; // Re-enable collisions so that we can unclip the object
                bool unclipped = AttemptToUnclip(depth: maxUnclippingAttempts, holdLayer, camTransform);
                if (!unclipped && Physics.CheckBox(_collider.bounds.center, _collider.bounds.extents * 0.1f, transform.rotation, ~(holdLayer | playerMask))) RespawnObject(); // If really stuck respawn
                rb.isKinematic = false;
                if (!rb.useGravity) rb.useGravity = true;
                if (isClipping) // Try to prevent object from flying
                {
                    if (rb.linearVelocity.sqrMagnitude > 0f) rb.linearVelocity = Vector3.zero;
                    if (rb.angularVelocity.sqrMagnitude > 0f) rb.angularVelocity = Vector3.zero;
                }
                else if (rb.linearVelocity != releaseVel) rb.linearVelocity = releaseVel;
            }
            int holdLayerIndex = ConvertLayerToIndex(holdLayer);
            gameObject.layer = held ? holdLayerIndex : startLayer;
            foreach (Transform child in childTransforms) child.gameObject.layer = gameObject.layer;
            if (isClipping) isClipping = false;
            return new InteractionStatus() { EndInteraction = !held }; // Return true when drop, false when pick up
        }
        else
        {
#if UNITY_EDITOR
            if (parameters[0] is not Transform) Debug.LogWarning($"WARNING: Parameter 0 needs to be the hold position transform. Received {parameters[0]} type {parameters[0].GetType()} as parameter 0");
            if (parameters[1] is not LayerMask) Debug.LogWarning($"WARNING: Parameter 1 needs to be the hold layer to render on. Received {parameters[1]} type {parameters[1].GetType()} as parameter 1");
            if (parameters[2] is not Collider) Debug.LogWarning($"WARNING: Parameter 2 needs to be the player collider. Received {parameters[2]} type {parameters[2].GetType()} as parameter 2");
            if (parameters[3] is not Transform) Debug.LogWarning($"WARNING: Parameter 3 needs to be the player camera transform. Received {parameters[3]} type {parameters[3].GetType()} as parameter 3");
#endif
            return new InteractionStatus() { EndInteraction = true }; // Drop object
        }
    }

    private bool IsGroundedInternal(System.Collections.Generic.HashSet<Holdable> checkedHoldables)
    {
        if (isClipping) return false; // If clipping then not grounded
        if (!isStartLayerAlsoGround) return Physics.CheckBox(_collider.bounds.center + Vector3.down * groundDistance, _collider.bounds.extents, transform.rotation, groundLayer); // If ground layer doesn't include own layer just do a simple check
        // If ground layer does include own layer do the following
        if (checkedHoldables.Contains(this)) return false; // If already checked this object return false
        checkedHoldables.Add(this); // Since this wasn't checked before make sure it won't be checked again
        System.Array.Clear(groundColliders, 0, groundColliders.Length);
        if (Physics.OverlapBoxNonAlloc(_collider.bounds.center + Vector3.down * groundDistance, _collider.bounds.extents, groundColliders, transform.rotation, groundLayer) > 1) // Ground check overlaps with more than seld
        {
            foreach (Collider groundCollider in groundColliders)
            {
                if (groundCollider == null) continue;
                // If the collider isn't holdable (normal ground) this object is grounded, or if checked holdables doesn't contain the holdable and it is grounded then this object is grounded
                if (groundCollider.TryGetComponent<Holdable>(out Holdable holdable))
                {
                    if (!checkedHoldables.Contains(holdable) && holdable.IsGroundedInternal(checkedHoldables)) return true;
                }
                else return true;
            }
        }
        return false; // We're not on the ground or touching something that is grounded then not grounded
    }

    private bool AttemptToUnclip(int depth, LayerMask holdLayer, Transform camTransform)
    {
        if (depth <= 0) return false; // Reached max depth
        int bitMask = ~(holdLayer | playerMask); // Layer that doesn't include the hold layer or player
        Vector3 placePos = transform.position;
        isClipping = false; // Reset is clipping for now to be able to exit early from checking collisions if needed
        UnblockLineOfSightToCamera(holdLayer, camTransform, bitMask, ref placePos);
        UnclipFromColliders(holdLayer, bitMask, ref placePos);
        if (placePos != transform.position)
        {
            transform.position = placePos;
            Physics.SyncTransforms();
        }
        if (!Physics.CheckBox(_collider.bounds.center, _collider.bounds.extents, transform.rotation, bitMask)) return true; // Successfully stopped clipping
        return AttemptToUnclip(--depth, holdLayer, camTransform); // Try again
    }

    private void UnblockLineOfSightToCamera(LayerMask holdLayer, Transform camTransform, int bitMask, ref Vector3 placePos)
    {
        float rayDistance = Vector3.Distance(camTransform.position, transform.position);
        if (Physics.Raycast(camTransform.position, camTransform.forward, out RaycastHit hit, rayDistance, bitMask)) // Something is blocking line of sight from the camera's forward to this holdable object
        {
            isClipping = true;
            if (Physics.Linecast(camTransform.position, transform.position, out RaycastHit boundHit, holdLayer)) // Hit on bounds of this holdable closest to camera
            {
                float boundsLength = Vector3.Distance(transform.position, boundHit.point); // Distance from the centre of this holdable object the bounds hit
                placePos = camTransform.position + camTransform.forward * (hit.distance - boundsLength); // Place this holdable object closer to the camera on the other side of the surface, place the bounds on the face of the surface to stop overlapping
            }
        }
    }

    private void UnclipFromColliders(LayerMask holdLayer, int bitMask, ref Vector3 placePos)
    {
        Vector3 totalOffset = Vector3.zero;
        int hitCount = 0;
        System.Array.Clear(clippingColliders, 0, clippingColliders.Length);
        int clipCount = Physics.OverlapBoxNonAlloc(_collider.bounds.center, _collider.bounds.extents, clippingColliders, transform.rotation, bitMask);
        if (!isClipping && clipCount == 0) return; // Not clipping at all
        foreach (Collider clippingCollider in clippingColliders)
        {
            if (clippingCollider == null) continue;
            isClipping = true;
            Vector3 closest = clippingCollider.ClosestPoint(transform.position);
            if (closest == transform.position) continue; // If the centre of the box is inside the collider skip it because the camera raycast will correct it
            if (Physics.Linecast(transform.position, closest, out RaycastHit closeHit, bitMask)) // Guarantee the point is on the surface and is the normal we should use
            {
                Vector3 outerPoint = (closeHit.point - transform.position).normalized * _collider.bounds.size.sqrMagnitude; // Point guaranteed to be outside the box's bounds
                if (Physics.Linecast(outerPoint, transform.position, out RaycastHit boundHit, holdLayer)) // Go from outside back inside to figure out what part of the box should touch the closest point
                {
                    float boundsLength = Vector3.Distance(transform.position, boundHit.point); // Distance from the centre of this holdable object the bounds hit
                    Vector3 offset = closeHit.normal.normalized * boundsLength; // The amount to offset this holdable object by to place its bounds on the surface clostest to the camera of the object it is overlapping with
                    totalOffset += offset;
                    hitCount++;
                }
            }
        }
        if (hitCount > 0) placePos += totalOffset / hitCount; // Average the displacement so it doesn't get placed to far away
    }

    public void LookAtPlayer(Vector3 playerPos) => transform.LookAt(new Vector3(playerPos.x, transform.position.y, playerPos.z));

    public void RespawnObject()
    {
        if (rb.isKinematic) rb.isKinematic = false;
        if (!rb.useGravity) rb.useGravity = true;
        if (rb.linearVelocity != Vector3.zero) rb.linearVelocity = Vector3.zero;
        if (rb.angularVelocity != Vector3.zero) rb.angularVelocity = Vector3.zero;
        if (transform.parent != startingParentTransform) transform.parent = startingParentTransform;
        if (transform.rotation != startRot) transform.rotation = startRot;
        if (transform.position != startPos) transform.position = startPos;
        if (gameObject.layer != startLayer) gameObject.layer = startLayer;
        if (held) held = false;
        if (isClipping) isClipping = false;
    }

    // Custom ConvertLayerToIndex is faster than using log to calculate
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    int ConvertLayerToIndex(LayerMask layer) // Will only do a single layer
    {
        int value = layer.value;
        int index = 0;
        while (value != 1)
        {
            value >>= 1; // Shift bits right by one (divide by 2)
            ++index;
        }
        return index;
    }
}