using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonMovement : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener playerAudio;
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
        if (playerCamera != null) playerCamera.enabled = false;
        if (playerAudio != null) playerAudio.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        bool local = IsClient && IsOwner;
        if (playerCamera != null) playerCamera.enabled = local;
        if (playerAudio != null) playerAudio.enabled = local;
        if (local) SetCursorLock(true);
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient && IsOwner) SetCursorLock(false);
    }

    private void Update()
    {
        if (!IsSpawned || !IsClient || !IsOwner || playerCamera == null) return;
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (keyboard == null || mouse == null) return;

        if (keyboard.escapeKey.wasPressedThisFrame) SetCursorLock(false);
        else if (mouse.leftButton.wasPressedThisFrame) SetCursorLock(true);

        bool locked = Cursor.lockState == CursorLockMode.Locked;
        if (locked)
        {
            Vector2 look = mouse.delta.ReadValue() * mouseSensitivity;
            transform.Rotate(Vector3.up * look.x);
            cameraPitch = Mathf.Clamp(cameraPitch - look.y, -lookLimit, lookLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }

        // Gravity continues even while the cursor is released.
        Vector2 input = Vector2.zero;
        if (locked)
        {
            if (keyboard.wKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed) input.y -= 1f;
            if (keyboard.dKey.isPressed) input.x += 1f;
            if (keyboard.aKey.isPressed) input.x -= 1f;
        }
        input = Vector2.ClampMagnitude(input, 1f);
        float speed = locked && keyboard.leftShiftKey.isPressed ? sprintSpeed : walkSpeed;
        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        if (locked && controller.isGrounded && keyboard.spaceKey.wasPressedThisFrame)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        verticalVelocity += gravity * Time.deltaTime;
        Vector3 horizontal = transform.right * input.x + transform.forward * input.y;
        CollisionFlags collisions = controller.Move(
            (horizontal * speed + Vector3.up * verticalVelocity) * Time.deltaTime);
        if ((collisions & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
            verticalVelocity = 0f;
    }

    private void SetCursorLock(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
