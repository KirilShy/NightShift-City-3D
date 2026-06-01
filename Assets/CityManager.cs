using UnityEngine;

// Singleton that tracks all city-wide statistics.
// Attach this to the CityManager GameObject.
// All other scripts access it via CityManager.Instance.
public class CityManager : MonoBehaviour
{
    // The one shared instance, set automatically in Awake.
    public static CityManager Instance;

    [Header("Trash Stats")]
    public int totalTrashSpawned = 0;  // Every piece of trash ever created.
    public int totalTrashCleaned = 0;  // Every piece of trash destroyed by CleanerBots.
    public int activeTrash       = 0;  // Trash currently on the map.

    [Header("Pothole Stats")]
    public int totalPotholesSpawned  = 0; // Every pothole ever created.
    public int totalPotholesRepaired = 0; // Every pothole destroyed by RepairBots.
    public int activePotholes        = 0; // Potholes currently on the map.

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ----- Trash registration -----------------------------------------------

    // Called by TrashSpawner whenever a new piece of trash is created.
    public void RegisterTrashSpawned()
    {
        totalTrashSpawned++;
        activeTrash++;
    }

    // Called by RobotController whenever a piece of trash is cleaned.
    public void RegisterTrashCleaned()
    {
        totalTrashCleaned++;
        activeTrash = Mathf.Max(0, activeTrash - 1);
    }

    // ----- Pothole registration ---------------------------------------------

    // Called by PotholeSpawner whenever a new pothole is created.
    public void RegisterPotholeSpawned()
    {
        totalPotholesSpawned++;
        activePotholes++;
    }

    // Called by RepairBotController whenever a pothole is repaired.
    public void RegisterPotholeRepaired()
    {
        totalPotholesRepaired++;
        activePotholes = Mathf.Max(0, activePotholes - 1);
    }

    // ----- City health -------------------------------------------------------

    // Trash lowers health by 3 per piece; potholes lower it by 8 per pothole
    // (potholes are more damaging — they are harder to fix and affect roads).
    // Clamped between 0 (destroyed city) and 100 (perfect).
    public int GetCityHealth()
    {
        return Mathf.Clamp(100 - activeTrash * 3 - activePotholes * 8, 0, 100);
    }
}
