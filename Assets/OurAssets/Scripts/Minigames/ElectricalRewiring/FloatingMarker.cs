using UnityEngine;

public class FloatingMarker : MonoBehaviour
{
    public float bobSpeed = 2f;
    public float bobHeight = 0.3f;
    public bool billboardToCamera = true;

    private Vector3 startWorldPos;
    private Camera mainCam;

    void Start()
    {
        startWorldPos = transform.position;
        mainCam = Camera.main;
    }

    void Update()
    {
        transform.position = startWorldPos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;

        if (billboardToCamera && mainCam != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position);
        }
    }
}