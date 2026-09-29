using UnityEngine;

public class PlayerDeathState : PlayerBaseState
{

    public PlayerDeathState(PlayerStateMachine playerStateMachine) : base(playerStateMachine){ }


    public override void Enter()
    {
        playerStateMachine.Ragdoll.ToggleRagdoll(true);
        playerStateMachine.WeaponDamage.gameObject.SetActive(false);

    }

    public override void Exit()
    {

    }

    public override void Tick(float deltaTime)
    {
       
    }
}
