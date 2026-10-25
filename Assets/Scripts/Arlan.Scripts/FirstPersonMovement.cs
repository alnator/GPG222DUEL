using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonMovement : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;

    [Header("Camera")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float lookLimit = 85f;

    private CharacterController controller;
    private float verticalVelocity;
    private float cameraPitch;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        SetCursorLock(true);
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            SetCursorLock(false);

        if (Mouse.current.leftButton.wasPressedThisFrame)
            SetCursorLock(true);

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Look();
            Move();
        }
    }

    private void Look()
    {
        Vector2 mouseInput =
            Mouse.current.delta.ReadValue() * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseInput.x);
        cameraPitch -= mouseInput.y;
        cameraPitch = Mathf.Clamp(cameraPitch, -lookLimit, lookLimit);

        playerCamera.localRotation =
            Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void Move()
    {
        Keyboard keyboard = Keyboard.current;

        Vector2 input = Vector2.zero;

        if (keyboard.wKey.isPressed) input.y += 1f;
        if (keyboard.sKey.isPressed) input.y -= 1f;
        if (keyboard.dKey.isPressed) input.x += 1f;
        if (keyboard.aKey.isPressed) input.x -= 1f;
        input = Vector2.ClampMagnitude(input, 1f);

        float speed = keyboard.leftShiftKey.isPressed
            ? sprintSpeed
            : walkSpeed;

        Vector3 movement =
            transform.right * input.x + transform.forward * input.y;

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (controller.isGrounded &&
            keyboard.spaceKey.wasPressedThisFrame)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity =
            movement * speed + Vector3.up * verticalVelocity;

        CollisionFlags collisions =
            controller.Move(velocity * Time.deltaTime);

        if ((collisions & CollisionFlags.Above) != 0 &&
            verticalVelocity > 0f)
        {
            verticalVelocity = 0f;
        }
    }

    private void SetCursorLock(bool locked)
    {
        Cursor.lockState = locked
            ? CursorLockMode.Locked
            : CursorLockMode.None;

        Cursor.visible = !locked;
    }
}