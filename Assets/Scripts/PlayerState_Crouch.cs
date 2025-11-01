// Concrete states: “Only decide and toggle; do not implement raw movement/physics here.”
using UnityEngine;

/// <summary>
///     Simple crouch: enter when control is held; exit when released.
///     Movement may be disabled entirely if 'disableMovementWhileCrouched' is true.
/// </summary>

public sealed class PlayerState_Crouch : PlayerBaseState
{
    public PlayerState_Crouch(PlayerController playerController, PlayerStateMachine stateMachine) : base(playerController, stateMachine) { }

    public override void Enter()
    {
        // Fire crouch entry trigger (for a small settle animation)
        playerController.Animator.ResetTrigger("crouchStart");
        playerController.Animator.SetTrigger("crouchStart");
        playerController.Animator.SetBool("isCrouching", true);

        // (Optional) player.StateFlags.AttackLock = true; // if you want to fully lock during crouch
    }

    public override void HandleInput()
    {
        // When player releases crouch, pop back to locomotion
        if (!playerController.Inputs.CrouchHeld) stateMachine.ChangeState(playerController.LocomotionState);

        // (Optional) allow attack while crouched by changing to AttackState here
        // if (player.Inputs.AttackPressed) machine.ChangeState(player.AttackState);
    }

    public override void LogicUpdate()
    {
        // While crouching, you can still check for attack if you want to allow it
        // (currently disabled; uncomment if you want attack from crouch)
        // if (playerController.Inputs.AttackPressed)
        //     stateMachine.ChangeState(playerController.AttackState);
    }

    public override void Exit()
    {
        playerController.Animator.SetBool("isCrouching", false);
        // PlayerController.StateFlags.AttackLock = false; // if you locked it in Enter()
    }
}
