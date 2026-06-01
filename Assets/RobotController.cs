using UnityEngine;

// Controls one robot: finds the nearest unclaimed trash, moves toward it,
// and destroys it when close enough.
// Attach this to each Robot GameObject.
public class RobotController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 3f;           // Units per second.
    public float rotationSpeed = 8f;   // How quickly the robot turns to face its target.

    [Header("Cleaning")]
    public float cleanDistance = 0.8f; // How close the robot must be to destroy trash.

    // The trash this robot is currently heading toward (null = searching).
    private TrashItem currentTrash;

    void Update()
    {
        // If we have no target, look for one every frame.
        // This is a simple approach — fine for small trash counts.
        if (currentTrash == null)
        {
            FindNearestUnclaimedTrash();
        }

        // Still nothing to do — idle until trash appears.
        if (currentTrash == null)
        {
            return;
        }

        MoveTowardTrash();
        TryCleanTrash();
    }

    // Release our claim if this robot is disabled or removed from the scene,
    // so the trash becomes available to another robot.
    void OnDestroy()
    {
        currentTrash?.Release();
    }

    // Rotate and translate toward the target, staying on the X/Z plane.
    void MoveTowardTrash()
    {
        // Safety: the trash object might have been destroyed by an external cause.
        if (currentTrash == null)
        {
            return;
        }

        // Keep the target at the same Y as this robot so we never fly up or sink.
        Vector3 targetPosition = currentTrash.transform.position;
        targetPosition.y = transform.position.y;

        Vector3 direction = targetPosition - transform.position;

        // Only rotate if we are not already on top of the target.
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );
    }

    // Check horizontal distance to trash and clean it if we are close enough.
    void TryCleanTrash()
    {
        if (currentTrash == null)
        {
            return;
        }

        // Flatten both positions to the X/Z plane before measuring distance
        // so height differences do not affect the check.
        Vector3 robotFlat = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 trashFlat = new Vector3(currentTrash.transform.position.x, 0f, currentTrash.transform.position.z);

        float distance = Vector3.Distance(robotFlat, trashFlat);

        if (distance <= cleanDistance)
        {
            CleanCurrentTrash();
        }
    }

    // Search all Trash-tagged objects and claim the nearest unclaimed one.
    void FindNearestUnclaimedTrash()
    {
        GameObject[] trashObjects = GameObject.FindGameObjectsWithTag("Trash");

        float closestDistance = Mathf.Infinity;
        TrashItem nearestTrash = null;

        foreach (GameObject trashObject in trashObjects)
        {
            TrashItem trashItem = trashObject.GetComponent<TrashItem>();

            // Skip objects that are missing the TrashItem component or already claimed.
            if (trashItem == null || trashItem.isClaimed)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, trashObject.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestTrash = trashItem;
            }
        }

        // Try to claim the nearest candidate. Claim() returns false if another
        // robot beat us to it on the same frame — in that case we wait until
        // the next Update to try again.
        if (nearestTrash != null && nearestTrash.Claim())
        {
            currentTrash = nearestTrash;
            Debug.Log(gameObject.name + " claimed trash at " + nearestTrash.transform.position);
        }
    }

    // Notify CityManager, destroy the trash object, and clear our target.
    void CleanCurrentTrash()
    {
        if (CityManager.Instance != null)
        {
            CityManager.Instance.RegisterTrashCleaned();
        }

        Debug.Log(gameObject.name + " cleaned trash");

        Destroy(currentTrash.gameObject);
        currentTrash = null;
    }
}
