// RobotLabel.cs — makes a world-space text label always face the camera.
//
// How it works:
//   TextMeshPro (3D) renders text as a mesh in the 3D scene.
//   By default it faces only one direction, so we rotate it every frame
//   to match the camera's orientation — this is called a "billboard" effect.
//
// Attach this to the Label child object inside each robot.

using UnityEngine;

public class RobotLabel : MonoBehaviour
{
    // LateUpdate runs after all other Updates and after camera movement,
    // which prevents the label from lagging one frame behind the camera.
    void LateUpdate()
    {
        if (Camera.main == null) return;

        // Copying the camera's rotation makes this object face the same
        // direction as the camera, so the text is always readable.
        transform.rotation = Camera.main.transform.rotation;
    }
}
