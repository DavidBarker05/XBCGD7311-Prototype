using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputHandler), typeof(PlayerInput))]
public class PlayerLook : MonoBehaviour
{
    [SerializeField, Min(0f)]
    float mouseHorizontalSensitivity = 0.2f;
    [SerializeField, Min(0f)]
    float mouseVerticalSensitivity = 0.225f;
    [SerializeField, Min(0f)]
    float controllerHorizontalSensitivity = 180f;
    [SerializeField, Min(0f)]
    float controllerVerticalSensitivity = 202.5f;
    [SerializeField, Range(-90f, 0f)]
    float minVerticalAngle = -80f;
    [SerializeField, Range(0f, 90f)]
    float maxVerticalAngle = 80f;

    PlayerInputHandler pih;
    Camera cam;

    float pitch;

    private void Awake()
    {
        pih = GetComponent<PlayerInputHandler>();
        cam = GetComponent<PlayerInput>().camera;
        LockMouse();
    }

    void Update()
    {
        float lX = pih.LookInput.x;
        float lY = pih.LookInput.y;
        float hSens = (pih.LookDevice is Mouse) ? mouseHorizontalSensitivity : (controllerHorizontalSensitivity * Time.deltaTime);
        float vSens = (pih.LookDevice is Mouse) ? mouseVerticalSensitivity : (controllerVerticalSensitivity * Time.deltaTime);
        float yaw = lX * hSens;
        pitch -= lY * vSens;
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
        cam.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        transform.Rotate(Vector3.up, yaw);
    }

    public void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockMouse()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
}
