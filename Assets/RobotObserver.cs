// RobotObserver.cs — attached to the Player.
// Each frame it finds the nearest robot within range and records a short
// status string. CityUI shows this text while in Citizen mode.
//
// This is proximity-based (no raycast, no colliders needed), which keeps it
// simple and robust — just walk near a robot to see what it's doing.

using UnityEngine;

public class RobotObserver : MonoBehaviour
{
    [Header("Observation")]
    public float observeRange = 6f; // how close you must be to read a robot

    // Read by CityUI. Empty string means "nothing nearby".
    public static string Info = "";

    void Update()
    {
        Info = "";
        float bestDist = observeRange;

        // Check CleanerBots.
        foreach (var bot in FindObjectsByType<RobotController>())
        {
            float d = Vector3.Distance(transform.position, bot.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                Info = bot.name + ": " + (bot.IsWorking ? "Cleaning trash" : "Searching");
            }
        }

        // Check RepairBots.
        foreach (var bot in FindObjectsByType<RepairBotController>())
        {
            float d = Vector3.Distance(transform.position, bot.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                Info = bot.name + ": " + (bot.IsWorking ? "Repairing pothole" : "Searching");
            }
        }
    }

    // Clear the static field when leaving play mode so it doesn't leak between runs.
    void OnDisable() => Info = "";
}
