using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

// One gun's data (model, where it sits in the hand, shooting + trail settings) and its shooting logic.
// Based on LlamAcademy's ScriptableObject gun series.
// PlayerGunSelector makes a runtime COPY of this asset before calling Spawn(), because Spawn() stores
// the spawned model and the trail pool on the object - the asset in the project stays clean.
[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]
public class GunScriptableObject : ScriptableObject
{
    public GunType Type;
    public string Name;
    public GameObject ModelPrefab;
    public Vector3 SpawnPoint;
    public Vector3 SpawnRotation;

    public ShootConfigScriptableObject ShootConfig;
    public TrailConfigScriptableObject TrailConfig;
    // optional: swaps Controller_Player's Pistol_* clips for this gun's own (leave empty for the pistol; a rifle would set one)
    public AnimatorOverrideController AnimatorOverride;

    // ---- runtime state, filled by Spawn() ----
    private MonoBehaviour ActiveMonoBehavior;   // runs the trail coroutines (ScriptableObjects can't)
    private GameObject Model;
    private float LastShootTime;
    private ParticleSystem ShootSystem;         // the muzzle flash; its position is where bullets start
    private ObjectPool<TrailRenderer> TrailPool;
    private GunFeedbacks Feedbacks;   // optional Feel feedbacks on the model prefab

    public Transform Muzzle => ShootSystem != null ? ShootSystem.transform : null;

    // Put the gun model in the hand (Parent) at this gun's SpawnPoint / SpawnRotation.
    public void Spawn(Transform Parent, MonoBehaviour ActiveMonoBehavior)
    {
        this.ActiveMonoBehavior = ActiveMonoBehavior;
        LastShootTime = float.NegativeInfinity;
        TrailPool = new ObjectPool<TrailRenderer>(CreateTrail, actionOnDestroy: trail => Destroy(trail.gameObject));

        Model = Instantiate(ModelPrefab);
        Model.transform.SetParent(Parent, false);
        Model.transform.localPosition = SpawnPoint;
        Model.transform.localRotation = Quaternion.Euler(SpawnRotation);

        ShootSystem = Model.GetComponentInChildren<ParticleSystem>();
        Feedbacks = Model.GetComponent<GunFeedbacks>();
    }

    // show/hide the spawned model (equip / holster)
    public void SetVisible(bool visible)
    {
        if (Model != null) Model.SetActive(visible);
    }

    // aimPoint = what the crosshair is on; the bullet leaves the muzzle and flies toward it.
    // Returns true when a shot actually fired (fire rate allowed it), so callers can add recoil.
    public bool Shoot(Vector3 aimPoint)
    {
        if (ShootSystem == null || Time.time < ShootConfig.FireRate + LastShootTime) return false;

        LastShootTime = Time.time;
        ShootSystem.Play();
        if (Feedbacks != null && Feedbacks.FireFeedback != null) Feedbacks.FireFeedback.PlayFeedbacks();

        // muzzle -> crosshair, plus a random wobble from Spread
        Vector3 shootDirection = (aimPoint - ShootSystem.transform.position).normalized
            + new Vector3(Random.Range(-ShootConfig.Spread.x, ShootConfig.Spread.x),
                          Random.Range(-ShootConfig.Spread.y, ShootConfig.Spread.y),
                          Random.Range(-ShootConfig.Spread.z, ShootConfig.Spread.z));

        shootDirection.Normalize();

        // the bullet is an instant raycast; the trail is just the visual flying out to where it hit (or off into the distance)
        if (Physics.Raycast(ShootSystem.transform.position, shootDirection, out RaycastHit hit,
            float.MaxValue, ShootConfig.HitMask, QueryTriggerInteraction.Ignore))   // ignore trigger volumes
        {
            ActiveMonoBehavior.StartCoroutine(PlayTrail(ShootSystem.transform.position,
                hit.point, hit));
        }
        else
        {
            ActiveMonoBehavior.StartCoroutine(PlayTrail(ShootSystem.transform.position,
                ShootSystem.transform.position + (shootDirection * TrailConfig.MissDistance),
                new RaycastHit()));
        }
        return true;
    }

    // Moves a pooled trail from the muzzle to the end point at SimulationSpeed, then returns it to the pool.
    private IEnumerator PlayTrail(Vector3 StartPoint, Vector3 EndPoint, RaycastHit Hit)
    {
        TrailRenderer instance = TrailPool.Get();
        instance.gameObject.SetActive(true);
        instance.transform.position = StartPoint;
        instance.Clear();
        yield return null; // wait a frame so the trail doesn't draw a streak from where it was last used
        instance.emitting = true;

        float distance = Vector3.Distance(StartPoint, EndPoint);
        float remainingDistance = distance;
        while(remainingDistance >0)
        {
            instance.transform.position = Vector3.Lerp(
                StartPoint, EndPoint, Mathf.Clamp01(1 - (remainingDistance / distance)));

            remainingDistance -= Mathf.Max(1f, TrailConfig.SimulationSpeed) * Time.deltaTime;

            yield return null;
        }

        instance.transform.position = EndPoint;

        if (Hit.collider != null)
        {
            // the bullet arrived: damage (Health) will go here.
            // (the tutorial's SurfaceManager/ImpactType comes from a separate LlamAcademy system we're skipping)
            if (Feedbacks != null && Feedbacks.ImpactFeedback != null) Feedbacks.ImpactFeedback.PlayFeedbacks(EndPoint);
        }
        // let the trail fade out completely before it goes back in the pool
        yield return new WaitForSeconds(TrailConfig.Duration);
        yield return null;
        instance.emitting = false;
        instance.gameObject.SetActive(false);
        TrailPool.Release(instance);
    }

    // remove the spawned model and pooled trails (PlayerGunSelector calls this when the player is destroyed)
    public void Despawn()
    {
        if (Model != null) Destroy(Model);
        TrailPool?.Clear();
    }

    // the pool calls this when it needs a new trail; it's built from TrailConfig
    private TrailRenderer CreateTrail()
    {
        GameObject instance = new GameObject("Bullet Trail");
        instance.transform.SetParent(ActiveMonoBehavior.transform, true);
        TrailRenderer trail = instance.AddComponent<TrailRenderer>();
        trail.colorGradient = TrailConfig.Color;
        trail.material = TrailConfig.Material;
        trail.widthCurve = TrailConfig.WidthCurve;
        trail.time = TrailConfig.Duration;
        trail.minVertexDistance = TrailConfig.MinVertexDistance;

        trail.emitting = false;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        return trail;
    }

}
