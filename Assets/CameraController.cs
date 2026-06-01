// CameraController.cs — uses the new Unity Input System.
// Attach to Main Camera. Works automatically when the city builder runs.
//
// Controls (Play Mode):
//   WASD / Arrow keys  — pan horizontally
//   Scroll wheel       — zoom
//   Q / E              — keyboard zoom out / in

using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Pan")]
    public float panSpeed  = 20f;

    [Header("Zoom")]
    public float zoomSpeed  = 25f;
    public float minHeight  = 8f;
    public float maxHeight  = 60f;

    void Update()
    {
        var kb    = Keyboard.current;
        var mouse = Mouse.current;

        // Guard: Input System devices may not be available on every platform.
        if (kb == null) return;

        // ---- Pan ------------------------------------------------------------
        Vector3 move = Vector3.zero;

        if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    move += FlatForward();
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  move -= FlatForward();
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  move -= transform.right;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) move += transform.right;
        move.y = 0f;

        if (move.sqrMagnitude > 0.01f)
            transform.position += move.normalized * panSpeed * Time.deltaTime;

        // ---- Zoom -----------------------------------------------------------
        float scroll = 0f;
        if (mouse != null)
            scroll = mouse.scroll.ReadValue().y * 0.01f; // scroll wheel

        if (kb.eKey.isPressed) scroll += Time.deltaTime * 0.6f; // zoom in
        if (kb.qKey.isPressed) scroll -= Time.deltaTime * 0.6f; // zoom out

        if (Mathf.Abs(scroll) > 0.0005f)
        {
            Vector3 newPos = transform.position + transform.forward * scroll * zoomSpeed;
            newPos.y = Mathf.Clamp(newPos.y, minHeight, maxHeight);
            transform.position = newPos;
        }
    }

    // Returns the camera's forward direction flattened to the XZ plane.
    Vector3 FlatForward()
    {
        Vector3 f = transform.forward;
        f.y = 0f;
        return f.sqrMagnitude > 0.001f ? f.normalized : Vector3.forward;
    }
}
