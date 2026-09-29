using UnityEngine;

// Works out which side of a character a hit came from, so hit reactions can play the matching
// animation (hit from the left -> reel to the right, hit from behind -> stumble forward).
// Animator states are named "<Reaction>From<Side>", e.g. "HurtFromLeft" or "StaggerFromBack".
public static class HitDirection
{
    public const string Front = "Front";
    public const string Back = "Back";
    public const string Left = "Left";
    public const string Right = "Right";

    public static string DirectionHitFrom(Transform victim, Vector3 hitFrom)
    {
        Vector3 local = victim.InverseTransformPoint(hitFrom);   // where the hit came from, in the victim's own frame

        if (Mathf.Abs(local.z) >= Mathf.Abs(local.x))
            return local.z >= 0f ? Front : Back;

        return local.x >= 0f ? Right : Left;
    }

    public static string StateName(string reaction, Transform victim, Vector3 hitFrom)
    {
        return reaction + "From" + DirectionHitFrom(victim, hitFrom);
    }
}
