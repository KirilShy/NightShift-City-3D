// CameraModeManager.cs — switches between the overview camera and the
// first-person citizen camera.
//
//   Press 1 → Overview mode (free-flying city camera, WASD pan / scroll zoom)
//   Press 2 → Citizen mode  (first-person player walking the streets)
//
// The builder wires all four references automatically. If you set the scene
// up by hand, drag the two cameras and two controllers into the Inspector.

using UnityEngine;
using UnityEngine.InputSystem;

public class CameraModeManager : MonoBehaviour
{
    [Header("Cameras")]
    public Camera overviewCamera;
    public Camera citizenCamera;

    [Header("Controllers")]
    public CameraController overviewController; // on the overview camera
    public PlayerController  playerController;   // on the player capsule

    // Read by CityUI to display the current mode in the HUD.
    public static string ModeName = "Overview";

    void Start()
    {
        // Start in overview so the player can see the whole city first.
        SetOverview();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit1Key.wasPressedThisFrame) SetOverview();
        if (kb.digit2Key.wasPressedThisFrame) SetCitizen();
    }

    public void SetOverview()
    {
        ModeName = "Overview";
        ToggleCamera(overviewCamera, true);
        ToggleCamera(citizenCamera, false);

        if (overviewController) overviewController.enabled = true;
        if (playerController)   playerController.enabled   = false;

        // Free the mouse cursor for overview navigation.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    public void SetCitizen()
    {
        if (citizenCamera == null) return; // no player in scene — stay in overview

        ModeName = "Citizen";
        ToggleCamera(overviewCamera, false);
        ToggleCamera(citizenCamera, true);

        if (overviewController) overviewController.enabled = false;
        if (playerController)   playerController.enabled   = true;

        // Lock and hide the cursor for first-person mouse-look.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    // Enables/disables a camera and its AudioListener together so only one
    // listener is ever active (avoids Unity's "multiple AudioListeners" warning).
    void ToggleCamera(Camera cam, bool on)
    {
        if (cam == null) return;
        cam.enabled = on;
        var al = cam.GetComponent<AudioListener>();
        if (al) al.enabled = on;
    }
}
