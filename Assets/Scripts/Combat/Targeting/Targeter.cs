using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class Targeter : MonoBehaviour
{
    [SerializeField] private CinemachineTargetGroup cineamchineTargetGroup;

    private List<Target> targets = new List<Target>();
    private Camera mainCamera;
    public Target CurrentTarget { get; private set; }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<Target>(out Target target)) return;

        targets.Add(target);
        target.OnDestroyed += RemoveTarget;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<Target>(out Target target)) return;

        RemoveTarget(target);

    }

    public bool SelectTarget()
    {
        if (targets.Count == 0) return false;

        Target closestTarget = null;
        float closestTargetDistance = Mathf.Infinity;


        foreach (Target target in targets)
        {
            Vector2 viewPos = mainCamera.WorldToViewportPoint(target.transform.position);

            if (!target.GetComponentInChildren<Renderer>().isVisible)
            {
                continue;
            }

            Vector2 toCentre = viewPos - new Vector2(0.5f, 0.5f); //centre of screen since screen is 1, 1

            if (toCentre.sqrMagnitude < closestTargetDistance)
            {
                closestTarget = target;
                closestTargetDistance = toCentre.sqrMagnitude;

            }

        }

        if (closestTarget == null) return false;


        CurrentTarget = closestTarget;
        cineamchineTargetGroup.AddMember(CurrentTarget.transform, 1f, .5f);
        return true;
    }

    public void SwitchTarget(int direction)
    {
        if (CurrentTarget == null) return;

        float currentX = mainCamera.WorldToViewportPoint(CurrentTarget.transform.position).x;
        Target nextTarget = null;
        float closestOffset = Mathf.Infinity;

        foreach (Target target in targets)
        {
            if (target == CurrentTarget) continue;

            Vector3 viewPos = mainCamera.WorldToViewportPoint(target.transform.position);
            if (viewPos.z < 0) continue; //behind the camera

            float offset = (viewPos.x - currentX) * direction;

            if (offset > 0 && offset < closestOffset)
            {
                nextTarget = target;
                closestOffset = offset;
            }
        }

        if (nextTarget == null) return;

        cineamchineTargetGroup.RemoveMember(CurrentTarget.transform);
        CurrentTarget = nextTarget;
        cineamchineTargetGroup.AddMember(CurrentTarget.transform, 1f, .5f);
    }

    public void Cancel()
    {
        if (CurrentTarget == null) return;

        cineamchineTargetGroup.RemoveMember(CurrentTarget.transform);
        CurrentTarget = null;
    }

    private void RemoveTarget(Target target)
    {
        if (CurrentTarget == target)
        {
            cineamchineTargetGroup.RemoveMember(CurrentTarget.transform);
            CurrentTarget = null;

        }

        target.OnDestroyed -= RemoveTarget;
        targets.Remove(target);


    }


}
