using UnityEngine;

// Makes a bot's indicator light sphere pulse in size, giving it a "living" look.
// Attach to the Light child sphere on any CleanerBot or RepairBot.
public class BotPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float minScale = 0.07f; // Smallest size during the pulse cycle.
    public float maxScale = 0.22f; // Largest size during the pulse cycle.
    public float speed    = 2.8f;  // How fast the pulse cycles.

    void Update()
    {
        // sin goes from -1 to +1; remap to 0..1 then lerp between min/max.
        float t = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;
        float s = Mathf.Lerp(minScale, maxScale, t);
        transform.localScale = new Vector3(s, s, s);
    }
}
