using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Camera))]
public class FlyCameraController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 30f;
    [SerializeField] float verticalSpeed = 20f;
    [SerializeField] float lookSensitivity = 2f;
    [SerializeField] float minPitch = -89f;
    [SerializeField] float maxPitch = 89f;

    CharacterController characterController;
    float pitch;
    float yaw;
    bool cursorLocked;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    void OnEnable()
    {
        PlayerInputGate.OnUIOpenChanged += HandleUIOpenChanged;
    }

    void OnDisable()
    {
        PlayerInputGate.OnUIOpenChanged -= HandleUIOpenChanged;
        UnlockCursor();
    }

    void Start()
    {
        Vector3 euler = transform.eulerAngles;
        pitch = euler.x > 180f ? euler.x - 360f : euler.x;
        yaw = euler.y;
    }

    void Update()
    {
        if (!PlayerInputGate.CameraControlsEnabled) {
            return;
        }

        HandleCursorLock();

        if (!cursorLocked) {
            return;
        }

        HandleLook();
        HandleMove();
    }

    void HandleUIOpenChanged(bool isOpen)
    {
        if (isOpen) {
            UnlockCursor();
        }
    }

    void HandleCursorLock()
    {
        if (!cursorLocked && Input.GetMouseButtonDown(0)) {
            LockCursor();
        }
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleMove()
    {
        Vector3 horizontal = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
            horizontal += transform.forward;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            horizontal -= transform.forward;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            horizontal += transform.right;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            horizontal -= transform.right;
        }

        horizontal.y = 0f;
        if (horizontal.sqrMagnitude > 1f) {
            horizontal.Normalize();
        }

        float vertical = 0f;
        if (Input.GetKey(KeyCode.Space)) {
            vertical += 1f;
        }
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
            vertical -= 1f;
        }

        Vector3 move = horizontal * moveSpeed + Vector3.up * vertical * verticalSpeed;
        characterController.Move(move * Time.deltaTime);
    }

    void LockCursor()
    {
        cursorLocked = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        cursorLocked = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
