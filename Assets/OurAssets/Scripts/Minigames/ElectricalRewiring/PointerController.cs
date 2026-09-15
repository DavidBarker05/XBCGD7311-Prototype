using UnityEngine;
using UnityEngine.InputSystem;

public class PointerController : MonoBehaviour
{

    public Transform pointA;
    public Transform pointB;
    public RectTransform safeZone;
    public float moveSpeed = 100f;

    [Header("Safe Zone Randomization")]
    public float minSafeZoneWidth = 50f;
    public float maxSafeZoneWidth = 150f;
    public bool randomizeWidth = true;

    private float direction = 1f;
    private RectTransform pointerTransform;
    private Vector3 targetPosition;

    private QTEManager qteManager;
    private bool isRunning;

    private QTEPlayerCharacter qtePlayer;
    private bool didQTEInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pointerTransform = GetComponent<RectTransform>();
        targetPosition = pointB.position;
    }

    public void Begin(QTEManager manager, QTEPlayerCharacter player)
    {
        qteManager = manager;
        qtePlayer = player;
        qtePlayer.OnQTEInput.AddListener(DoQTEInput);
        RandomizeSafeZone();
        isRunning = true;
    }

    void RandomizeSafeZone()
    {
        // Work in the local space of the safeZone's parent, assuming
        // pointA, pointB and safeZone share the same parent RectTransform.
        RectTransform parent = safeZone.parent as RectTransform;

        float aX = parent.InverseTransformPoint(pointA.position).x;
        float bX = parent.InverseTransformPoint(pointB.position).x;

        float minX = Mathf.Min(aX, bX);
        float maxX = Mathf.Max(aX, bX);

        float width = randomizeWidth
            ? Random.Range(minSafeZoneWidth, maxSafeZoneWidth)
            : safeZone.rect.width;

        // Resize the safe zone if we're randomizing width
        if (randomizeWidth)
        {
            Vector2 size = safeZone.sizeDelta;
            size.x = width;
            safeZone.sizeDelta = size;
        }

        float halfWidth = width * 0.5f;

        // Keep the safe zone fully within the track bounds
        float randomX = Random.Range(minX + halfWidth, maxX - halfWidth);

        Vector2 anchoredPos = safeZone.anchoredPosition;
        anchoredPos.x = randomX;
        safeZone.anchoredPosition = anchoredPos;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isRunning) return;

        pointerTransform.position = Vector3.MoveTowards(pointerTransform.position, targetPosition, moveSpeed * Time.unscaledDeltaTime);

        if (Vector3.Distance(pointerTransform.position, pointA.position) < 0.1f)
        {
            targetPosition = pointB.position;
            direction = 1f;
        }

        else if (Vector3.Distance(pointerTransform.position, pointB.position) < 0.1f)
        {
            targetPosition = pointA.position;
            direction = -1f;
        }

        //if (Input.GetKeyDown(KeyCode.Space))
        if (didQTEInput)
        {
            didQTEInput = false;
            CheckSuccess();
        }
    }

    void DoQTEInput()
    {
        didQTEInput = true;
    }

    void CheckSuccess()
    {
        qtePlayer.OnQTEInput.RemoveListener(DoQTEInput);
        qtePlayer = null;
        if (RectTransformUtility.RectangleContainsScreenPoint(safeZone, pointerTransform.position, null))
        {
            Debug.Log("Success!");
            qteManager.Success();
        }
        else
        {
            Debug.Log("Failure!");
            qteManager.Falilure();
        }
    }
}