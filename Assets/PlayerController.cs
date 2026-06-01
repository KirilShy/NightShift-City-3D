// PlayerController.cs — simple first-person citizen controller.
// Uses Unity's new Input System (this project has it enabled).
//
// Controls:
//   WASD        — move
//   Mouse       — look around
//   Left Shift  — sprint
//   Space       — jump
//
// Requires a CharacterController (added automatically below) for ground
// collision and gravity. The capsule visual and camera are children,
// created by the city builder.

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed   = 4f;
    public float sprintSpeed = 8f;

    [Header("Jump / Gravity")]
    public float jumpHeight = 1.2f;
    public float gravity    = -20f;

    [Header("Look")]
    public float mouseSensitivity = 0.10f;

    // The camera transform — tilted up/down for pitch. Assigned by the builder.
    public Transform cameraPivot;

    private CharacterController _cc;
    private float _pitch = 0f;            // current up/down look angle
    private float _verticalVelocity = 0f; // gravity / jump velocity

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        var kb    = Keyboard.current;
        var mouse = Mouse.current;
        if (kb == null) return;

        HandleLook(mouse);
        HandleMove(kb);
    }

    void HandleLook(Mouse mouse)
    {
        if (mouse == null || cameraPivot == null) return;

        Vector2 delta = mouse.delta.ReadValue() * mouseSensitivity;

        // Yaw — rotate the whole body left/right.
        transform.Rotate(Vector3.up, delta.x);

        // Pitch — tilt only the camera up/down, clamped so we can't flip over.
        _pitch = Mathf.Clamp(_pitch - delta.y, -80f, 80f);
        cameraPivot.localEulerAngles = new Vector3(_pitch, 0f, 0f);
    }

    void HandleMove(Keyboard kb)
    {
        float speed = kb.leftShiftKey.isPressed ? sprintSpeed : walkSpeed;

        // Build a movement direction from WASD relative to where we face.
        float x = 0f, z = 0f;
        if (kb.wKey.isPressed) z += 1f;
        if (kb.sKey.isPressed) z -= 1f;
        if (kb.dKey.isPressed) x += 1f;
        if (kb.aKey.isPressed) x -= 1f;

        Vector3 move = transform.right * x + transform.forward * z;
        if (move.sqrMagnitude > 1f) move.Normalize(); // no faster diagonals

        // Gravity and jumping.
        if (_cc.isGrounded)
        {
            _verticalVelocity = -1f; // small downward force keeps us grounded
            if (kb.spaceKey.wasPressedThisFrame)
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else
        {
            _verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 velocity = move * speed + Vector3.up * _verticalVelocity;
        _cc.Move(velocity * Time.deltaTime);
    }
}
