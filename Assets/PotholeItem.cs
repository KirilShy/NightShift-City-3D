using UnityEngine;

// Attached to every Pothole prefab instance.
// Tracks whether a RepairBot has already claimed this pothole
// so two RepairBots never chase the same target.
public class PotholeItem : MonoBehaviour
{
    // True once a RepairBot has reserved this pothole for itself.
    public bool isClaimed = false;

    // Call this to reserve the pothole for a RepairBot.
    // Returns true if the claim succeeded, false if already taken.
    public bool Claim()
    {
        if (isClaimed) return false;
        isClaimed = true;
        return true;
    }

    // Call this to release the claim so another RepairBot can pick it up.
    public void Release()
    {
        isClaimed = false;
    }
}
