// Concrete states: “Only decide and toggle; do not implement raw movement/physics here.”
using UnityEngine;
/// <summary>
///     Jump is a short-lived state: we give the player an initial upward velocity,
///     then immediately hand control back to Locomotion once the jump has started.
///     (Gravity/landing is handled by the controller; the Animator uses isGrounded to transition.)
/// </summary>
public sealed class PlayerState_Jump : PlayerBaseState
{
    public PlayerState_Jump(PlayerController playerController, PlayerStateMachine stateMachine) : base(playerController, stateMachine) { }

    public override void Enter()
    {
        // Classic physics formula: v = sqrt(2 * g * h) ; gravity is negative
        playerController.SetVerticalVelocity(Mathf.Sqrt(2f * -playerController.Gravity * playerController.JumpHeight));

        // optional: lock certain actions during jump
        // playerController.StateFlags.AttackLock = true;
    }

    public override void LogicUpdate()
    {
        // return to locomotion when grounded again
        if (playerController.IsGrounded && playerController.VerticalVelocity <= 0f)
        {
            stateMachine.ChangeState(playerController.LocomotionState);
        }
    }

    public override void Exit()
    {
        // unlock if you locked in Enter
        // playerController.StateFlags.AttackLock = false;
    }
}
