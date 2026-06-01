using UnityEngine;

// Singleton that tracks city-wide trash statistics.
// Attach this to the CityManager GameObject.
// Other scripts access it via CityManager.Instance.
public class CityManager : MonoBehaviour
{
    // The single shared instance — set automatically in Awake.
    public static CityManager Instance;

    [Header("Stats (read-only in play mode)")]
    public int totalTrashSpawned = 0;  // All trash ever created this session.
    public int totalTrashCleaned = 0;  // All trash destroyed by robots.
    public int activeTrash = 0;        // Trash currently on the map.

    void Awake()
    {
        // Simple singleton: whichever CityManager loads first wins.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Called by TrashSpawner each time a new piece of trash is created.
    public void RegisterTrashSpawned()
    {
        totalTrashSpawned++;
        activeTrash++;
    }

    // Called by RobotController each time a piece of trash is destroyed.
    public void RegisterTrashCleaned()
    {
        totalTrashCleaned++;
        // Clamp to zero in case of any unexpected double-clean.
        activeTrash = Mathf.Max(0, activeTrash - 1);
    }

    // Returns a 0–100 cleanliness score (100 = spotless, drops by 5 per active piece of trash).
    public int GetCityHealth()
    {
        return Mathf.Clamp(100 - activeTrash * 5, 0, 100);
    }


}
