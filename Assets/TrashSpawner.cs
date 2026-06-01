using UnityEngine;

// Periodically spawns Trash prefabs at random positions on the X/Z plane.
// Attach this to the TrashSpawner GameObject.
public class TrashSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject trashPrefab; // Drag your Trash prefab here in the Inspector.

    [Header("Spawning")]
    public float spawnInterval = 5f;  // Seconds between each new piece of trash.
    public float mapRange = 20f;      // Trash spawns within (-mapRange, mapRange) on X and Z.
    public float spawnY = 0.5f;       // Height at which trash appears (should sit just above ground).

    private float timer = 0f;

    void Start()
    {
        // Spawn one piece immediately so robots have something to do right away.
        SpawnTrash();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnTrash();
            timer = 0f;
        }
    }

    void SpawnTrash()
    {
        if (trashPrefab == null)
        {
            Debug.LogError("TrashSpawner: trashPrefab is not assigned in the Inspector!");
            return;
        }

        float x = Random.Range(-mapRange, mapRange);
        float z = Random.Range(-mapRange, mapRange);
        Vector3 spawnPosition = new Vector3(x, spawnY, z);

        GameObject newTrash = Instantiate(trashPrefab, spawnPosition, Quaternion.identity);
        newTrash.name = "Trash";
        newTrash.tag = "Trash";

        // Make sure the TrashItem component exists (add it if the prefab is missing it).
        TrashItem trashItem = newTrash.GetComponent<TrashItem>();
        if (trashItem == null)
        {
            trashItem = newTrash.AddComponent<TrashItem>();
        }

        // Ensure the new trash starts unclaimed regardless of prefab state.
        trashItem.isClaimed = false;

        if (CityManager.Instance != null)
        {
            CityManager.Instance.RegisterTrashSpawned();
        }

        Debug.Log("Spawned trash at " + spawnPosition);
    }
}
