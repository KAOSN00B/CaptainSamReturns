using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class PlayerBlockingState : PlayerBaseState
{
    public PlayerBlockingState(PlayerStateMachine playerStateMachine) : base(playerStateMachine) { }

    private readonly int BlockHash = Animator.StringToHash("Blocking");

    private const float CrossFadeDuration = .1f;

    public override void Enter()
    {
        playerStateMachine.GunSelector.Unequip();   // you block with the sword, so it comes back out (before the crossfade)
        playerStateMachine.Health.SetBlocking(true);
        playerStateMachine.Animator.CrossFadeInFixedTime(BlockHash, CrossFadeDuration);
    }

    public override void Exit()
    {
        playerStateMachine.Health.SetBlocking(false);
    }

    public override void Tick(float deltaTime)
    {
        Move(deltaTime);
        
        if(!playerStateMachine.InputReader.IsBlocking)
        {
            ReturnToLocomotion();
            return;
        }

    }

    
}
