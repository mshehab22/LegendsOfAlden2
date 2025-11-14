// Concrete states: “Only decide and toggle; do not implement raw movement/physics here.”

/// <summary>
///     Default movement state (walk/run/idle).
///     Only decides about transitions; the actual movement is handled in PlayerController.Update.
/// </summary>

public class PlayerState_Locomotion : PlayerBaseState
{
    public PlayerState_Locomotion(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        playerController.StateFlags.AttackLock = false;  // Ensure movement and rotation is unlocked when we return from other states
    }

    public override void Tick(float deltaTime)
    {
        // Jump pressed +grounded->go to jump
        if (playerController.Inputs.JumpPressed &&
            playerController.IsGrounded &&
            !playerController.Inputs.CrouchHeld &&
            !playerController.StateFlags.AttackLock)
        {
            stateMachine.SwitchState(playerController.JumpState);
            return;
        }

        // Crouch held/pressed -> go to crouch
        if (playerController.Inputs.CrouchPressed || playerController.Inputs.CrouchHeld)
        {
            stateMachine.SwitchState(playerController.CrouchState);
            return;
        }

        // Attack pressed -> go to attack (movement may be locked there)
        if (playerController.Inputs.AttackPressed)
        {
            stateMachine.SwitchState(playerController.AttackState);
            return;
        }
    }

    public override void Exit() { }
}


