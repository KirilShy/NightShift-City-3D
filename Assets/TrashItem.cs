using UnityEngine;

// Attached to every Trash prefab instance.
// Tracks whether a robot has already claimed this piece of trash
// so two robots never chase the same target.
public class TrashItem : MonoBehaviour
{
    // True once a robot has reserved this trash for itself.
    // Other robots skip trash items where this is true.
    public bool isClaimed = false;

    // Call this to reserve the trash for a robot.
    // Returns true if the claim succeeded, false if already taken.
    public bool Claim()
    {
        if (isClaimed)
        {
            return false; // Someone else already claimed it.
        }

        isClaimed = true;
        return true;
    }

    // Call this to release the claim so another robot can pick it up.
    public void Release()
    {
        isClaimed = false;
    }
}
