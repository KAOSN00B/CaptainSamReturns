using UnityEngine;
using UnityEngine.Animations.Rigging;

// Puts the gun on the crosshair while aiming, using Unity's Animation Rigging package.
//
// How it fits together:
//   CaptainSam has a RigBuilder with one rig, "GunAimRig". That rig has two Two Bone IK constraints:
//     RightArm -> pulls the right hand (holding the gun) to RightArmTarget
//     LeftArm  -> pulls the left hand onto the gun's "LeftHand" grip point (two-handed hold)
//   This script moves those targets every frame. The rig then bends the arms to reach them.
//   When you're not aiming, the rig's weight fades to 0, so the normal animations (sword swings etc.) are untouched.
//
// The aiming state calls SetAim() every frame with the point under the crosshair, and Kick() on every shot.
[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
[DefaultExecutionOrder(50)]   // after the state machine's Update has set this frame's aim point
public class PlayerIK : MonoBehaviour
{
    [Header("Support hand (filled in by PlayerGunSelector from the gun's grip points)")]
    public Transform LeftHandIKTarget;                       // the gun's "LeftHand" child: where the left hand grabs
    public Transform LeftElbowIKTarget;                      // optional: the gun's "LeftElbow" child
    [Range(0f, 1f)] public float HandIKAmount = 1f;          // how strongly the left hand sticks to the grip
    [Range(0f, 1f)] public float ElbowIKAmount = 0.5f;       // how strongly the left elbow follows a LeftElbow point

    [Header("Aiming pose")]
    [Min(0f)] public float AimBlendSpeed = 8f;               // how fast the aim pose fades in/out (1 / seconds)
    [Range(0f, 1f)] public float AimHandWeight = 1f;         // how strongly the gun hand follows the aim
    [Min(0.1f)] public float AimArmReach = 0.56f;            // how far in front of the shoulder the gun hand sits (metres)
    public Vector3 AimAnchorOffset = new Vector3(0f, -0.1f, 0f);            // moves the point the arms aim from (player space); down = hands lower than the eyes
    public Vector3 AimElbowHintOffset = new Vector3(0.25f, -0.3f, 0f);      // gun-arm elbow: out to the right and down
    public Vector3 SupportElbowHintOffset = new Vector3(-0.25f, -0.35f, 0f); // support-arm elbow: out to the left and down
    [Range(0f, 1f)] public float AimElbowHintWeight = 0.5f;  // how strongly the elbows follow those hints

    [Header("Recoil")]
    [Min(0f)] public float RecoilRecoverSpeed = 9f;          // how fast the arms settle after a kick (1 / seconds)

    [Header("Rig (on CaptainSam/GunAimRig)")]
    [SerializeField] private Rig aimRig;
    [SerializeField] private TwoBoneIKConstraint rightArm;
    [SerializeField] private TwoBoneIKConstraint leftArm;

    // keeps the arm from locking dead straight, which looks robotic and makes IK snap
    private const float MaxReachFraction = 0.94f;
    // the gun's barrel sits away from the wrist, so the hand is re-aimed from the barrel a few times until it lines up
    private const int BarrelAlignPasses = 3;
    // aiming almost straight up/down: switch the "up" reference so the gun can't spin
    private const float NearlyVerticalDot = 0.98f;
    // directions shorter than this are treated as zero (avoids NaN rotations)
    private const float TinyDistanceSqr = 0.0001f;
    // elbow hints are placed halfway along the arm before being pushed out
    private const float ElbowHintAlongArm = 0.5f;

    public float AimWeight => aimWeight;

    private Animator animator;
    private Transform gunHand;        // right hand bone (the gun is parented to it)
    private Transform gunShoulder;    // right upper arm bone
    private Transform supportShoulder;// left upper arm bone
    private float maxReach;           // longest reach that still keeps a little bend in the arm

    private bool aiming;
    private Vector3 aimPoint;
    private float aimWeight;          // 0 = normal animation, 1 = full aim pose

    // where the muzzle sits relative to the hand bone (measured once when the gun spawns)
    private Vector3 muzzleInHand;
    private Quaternion muzzleRotationInHand = Quaternion.identity;

    private float recoil;             // 1 right after a shot, settles back to 0
    private float recoilBack;
    private float recoilAngle;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        gunHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        gunShoulder = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        supportShoulder = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);

        Transform gunElbow = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        if (gunShoulder != null && gunElbow != null && gunHand != null)
        {
            float armLength = Vector3.Distance(gunShoulder.position, gunElbow.position)
                            + Vector3.Distance(gunElbow.position, gunHand.position);
            maxReach = armLength * MaxReachFraction;
        }

        ResetAim();
    }

    private void OnDisable() => ResetAim();

    // ---------- called by other scripts ----------

    // Remember where the gun's muzzle is relative to the hand, so the hand can be turned to point the barrel.
    public void SetWeapon(Transform muzzle)
    {
        if (gunHand == null || muzzle == null) return;

        muzzleInHand = gunHand.InverseTransformPoint(muzzle.position);
        muzzleRotationInHand = Quaternion.Inverse(gunHand.rotation) * muzzle.rotation;
    }

    // Called every frame by the aiming state. When aiming stops, the last point is kept so the pose fades out smoothly.
    public void SetAim(bool isAiming, Vector3 point)
    {
        aiming = isAiming;
        if (isAiming) aimPoint = point;
    }

    // Drop the aim pose instantly (used when the sword comes back out).
    public void ResetAim()
    {
        aiming = false;
        aimWeight = 0f;
        if (aimRig != null) aimRig.weight = 0f;
    }

    // One shot's kick: the gun hand jumps back and the muzzle flips up, then settles.
    public void Kick(float kickBack, float kickAngle)
    {
        recoil = 1f;
        recoilBack = kickBack;
        recoilAngle = kickAngle;
    }

    // ---------- every frame ----------

    private void Update()
    {
        if (aimRig == null || rightArm == null || gunHand == null || gunShoulder == null) return;

        aimWeight = Mathf.MoveTowards(aimWeight, aiming ? 1f : 0f, AimBlendSpeed * Time.deltaTime);
        aimRig.weight = aimWeight;
        recoil = Mathf.MoveTowards(recoil, 0f, RecoilRecoverSpeed * Time.deltaTime);
        if (aimWeight <= 0f) return;

        Vector3 anchor = gunShoulder.position + transform.TransformDirection(AimAnchorOffset);
        Vector3 toAimPoint = aimPoint - anchor;
        if (toAimPoint.sqrMagnitude < TinyDistanceSqr) return;

        float reach = Mathf.Min(AimArmReach, maxReach);
        Vector3 aimDirection = toAimPoint.normalized;
        Vector3 handPosition = anchor + aimDirection * reach;
        Quaternion handRotation = PointBarrelAtAimPoint(handPosition, ref aimDirection);

        ApplyRecoil(aimDirection, ref handPosition, ref handRotation);

        DriveGunArm(anchor, aimDirection, reach, handPosition, handRotation);
        DriveSupportArm(handPosition, handRotation);
    }

    // Turn the hand so the barrel (not the wrist) points at the aim point.
    // The muzzle is ~30 cm in front of the wrist, so aiming the wrist alone would miss slightly:
    // each pass re-aims from where the muzzle would end up, which converges in a couple of passes.
    private Quaternion PointBarrelAtAimPoint(Vector3 handPosition, ref Vector3 aimDirection)
    {
        Quaternion handRotation = Quaternion.identity;

        for (int pass = 0; pass < BarrelAlignPasses; pass++)
        {
            bool nearlyVertical = Mathf.Abs(Vector3.Dot(aimDirection, Vector3.up)) > NearlyVerticalDot;
            Vector3 up = nearlyVertical ? transform.forward : Vector3.up;

            // the rotation that makes the barrel face aimDirection, converted to the hand bone's rotation
            handRotation = Quaternion.LookRotation(aimDirection, up) * Quaternion.Inverse(muzzleRotationInHand);

            Vector3 muzzlePosition = handPosition + handRotation * Vector3.Scale(muzzleInHand, gunHand.lossyScale);
            Vector3 muzzleToAim = aimPoint - muzzlePosition;
            if (muzzleToAim.sqrMagnitude > TinyDistanceSqr) aimDirection = muzzleToAim.normalized;
        }

        return handRotation;
    }

    // Recoil: the hand slides back along the aim line and the muzzle pitches up, scaled by how fresh the kick is.
    private void ApplyRecoil(Vector3 aimDirection, ref Vector3 handPosition, ref Quaternion handRotation)
    {
        if (recoil <= 0f) return;

        Vector3 pitchAxis = Vector3.Cross(Vector3.up, aimDirection);   // the gun's "right" axis
        if (pitchAxis.sqrMagnitude > TinyDistanceSqr)
            handRotation = Quaternion.AngleAxis(-recoilAngle * recoil, pitchAxis.normalized) * handRotation;   // negative = muzzle up

        handPosition -= aimDirection * (recoilBack * recoil);
    }

    // Right arm: hand to the solved pose, elbow pushed out and down so it bends naturally.
    private void DriveGunArm(Vector3 anchor, Vector3 aimDirection, float reach, Vector3 handPosition, Quaternion handRotation)
    {
        var data = rightArm.data;
        data.target.SetPositionAndRotation(handPosition, handRotation);
        if (data.hint != null)
            data.hint.position = anchor + aimDirection * (reach * ElbowHintAlongArm) + transform.TransformDirection(AimElbowHintOffset);
        data.hintWeight = AimElbowHintWeight;
        rightArm.data = data;
        rightArm.weight = AimHandWeight;
    }

    // Left arm: onto the gun's grip point. The grip is a child of the gun (which rides on the right hand),
    // so its pose is predicted from the right hand's NEW pose rather than read from last frame - otherwise it lags a frame.
    private void DriveSupportArm(Vector3 handPosition, Quaternion handRotation)
    {
        if (leftArm == null) return;

        bool hasGrip = LeftHandIKTarget != null;
        leftArm.weight = hasGrip ? HandIKAmount : 0f;
        if (!hasGrip) return;

        Vector3 gripOffsetInHand = gunHand.InverseTransformPoint(LeftHandIKTarget.position);
        Quaternion gripRotationInHand = Quaternion.Inverse(gunHand.rotation) * LeftHandIKTarget.rotation;

        var data = leftArm.data;
        Vector3 gripPosition = handPosition + handRotation * Vector3.Scale(gripOffsetInHand, gunHand.lossyScale);
        data.target.SetPositionAndRotation(gripPosition, handRotation * gripRotationInHand);

        if (data.hint != null)
        {
            if (LeftElbowIKTarget != null)
            {
                data.hint.position = LeftElbowIKTarget.position;
            }
            else if (supportShoulder != null)
            {
                Vector3 halfway = Vector3.Lerp(supportShoulder.position, gripPosition, ElbowHintAlongArm);
                data.hint.position = halfway + transform.TransformDirection(SupportElbowHintOffset);
            }
        }
        data.hintWeight = LeftElbowIKTarget != null ? ElbowIKAmount : AimElbowHintWeight;
        leftArm.data = data;
    }
}
