using MoreMountains.Feedbacks;
using UnityEngine;

// Lives on a gun's model prefab: the Feel feedbacks that gun plays.
// GunScriptableObject finds it after spawning the model, so each gun brings its own feel.
[DisallowMultipleComponent]
public class GunFeedbacks : MonoBehaviour
{
    [field: SerializeField] public MMF_Player FireFeedback { get; private set; }     // on every shot: sound, small kick
    [field: SerializeField] public MMF_Player ImpactFeedback { get; private set; }   // where the bullet lands: sparks
}
