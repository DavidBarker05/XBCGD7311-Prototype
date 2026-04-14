using UnityEngine;
using UnityEngine.InputSystem;

public class PointerController : MonoBehaviour
{

    public Transform pointA;
    public Transform pointB;
    public RectTransform safeZone;
    public float moveSpeed = 100f;

    private float direction = 1f;
    private RectTransform pointerTransform;
    private Vector3 targetPosition;

    private QTEManager qteManager;
    private bool isRunning;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pointerTransform = GetComponent<RectTransform>();
        targetPosition = pointB.position;
    }

    public void Begin(QTEManager manager)
    {
        qteManager = manager;
        isRunning = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isRunning) return;
        pointerTransform.position = Vector3.MoveTowards(pointerTransform.position, targetPosition, moveSpeed * Time.unscaledDeltaTime);

        if(Vector3.Distance(pointerTransform.position, pointA.position) < 0.1f)
        {
            targetPosition = pointB.position;
            direction = 1f;
        }

        else if(Vector3.Distance(pointerTransform.position, pointB.position) < 0.1f)
        {
            targetPosition = pointA.position;
            direction = -1f;
        }

        //if (Input.GetKeyDown(KeyCode.Space))
        if (InputSystem.actions.FindAction("Jump").WasPerformedThisFrame())
        {
            CheckSuccess();
        }
    }

    void CheckSuccess()
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(safeZone, pointerTransform.position, null))
        {
            Debug.Log("Success!");
        }
        else
        {
            Debug.Log("Failure!");
        }
    }
}
