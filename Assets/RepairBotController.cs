using UnityEngine;

// Controls one RepairBot: finds the nearest unclaimed pothole, moves toward it,
// and destroys it when close enough.
// Attach this to the RepairBot parent GameObject.
// RepairBots completely ignore trash — that is CleanerBot's job.
public class RepairBotController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2.5f;          // Units per second (slightly slower than CleanerBots).
    public float rotationSpeed = 7f;    // How quickly the bot turns toward its target.

    [Header("Repairing")]
    public float repairDistance = 0.8f; // How close the bot must be to repair the pothole.

    // The pothole this bot is currently heading toward (null = searching).
    private PotholeItem currentPothole;

    // True when this bot has a pothole target. Read by RobotObserver for status text.
    public bool IsWorking => currentPothole != null;

    void Update()
    {
        // Look for a pothole if we don't have a target.
        if (currentPothole == null)
            FindNearestUnclaimedPothole();

        if (currentPothole == null) return;

        MoveToPothole();
        TryRepair();
    }

    // Release the claim if this bot is removed from the scene mid-game.
    void OnDestroy()
    {
        currentPothole?.Release();
    }

    // Rotate and move toward the pothole on the X/Z plane only.
    void MoveToPothole()
    {
        if (currentPothole == null) return;

        Vector3 target = currentPothole.transform.position;
        target.y = transform.position.y; // Keep Y constant — no flying or sinking.

        Vector3 direction = target - transform.position;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                rotationSpeed * Time.deltaTime);
        }

        transform.position = Vector3.MoveTowards(
            transform.position, target, speed * Time.deltaTime);
    }

    // Check horizontal distance and repair when close enough.
    void TryRepair()
    {
        if (currentPothole == null) return;

        Vector3 flat1 = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 flat2 = new Vector3(currentPothole.transform.position.x, 0f, currentPothole.transform.position.z);

        if (Vector3.Distance(flat1, flat2) <= repairDistance)
            RepairCurrentPothole();
    }

    // Search all Pothole-tagged objects and claim the nearest unclaimed one.
    void FindNearestUnclaimedPothole()
    {
        GameObject[] potholes = GameObject.FindGameObjectsWithTag("Pothole");

        float closest = Mathf.Infinity;
        PotholeItem nearest = null;

        foreach (GameObject obj in potholes)
        {
            PotholeItem item = obj.GetComponent<PotholeItem>();
            if (item == null || item.isClaimed) continue;

            float dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist < closest)
            {
                closest = dist;
                nearest = item;
            }
        }

        if (nearest != null && nearest.Claim())
        {
            currentPothole = nearest;
            Debug.Log(gameObject.name + " claimed pothole at " + nearest.transform.position);
        }
    }

    // Tell CityManager, destroy the pothole, and clear the target.
    void RepairCurrentPothole()
    {
        if (CityManager.Instance != null)
            CityManager.Instance.RegisterPotholeRepaired();

        Debug.Log(gameObject.name + " repaired pothole");

        Destroy(currentPothole.gameObject);
        currentPothole = null;
    }
}
