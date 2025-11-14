// Concrete states: “Only decide and toggle; do not implement raw movement/physics here.”
/// <summary>
///     Plays an upper-body attack and locks locomotion/rotation while the attack is tagged "Attack".
///     Returns to locomotion when the attack layer is no longer in an "Attack" state nor transitioning.
/// </summary>

public sealed class PlayerState_Attack : PlayerBaseState
{
    public PlayerState_Attack(PlayerStateMachine stateMachine) : base(stateMachine) { }
    public override void Enter()
    {
        playerController.StateFlags.AttackLock = true; // Lock movement/rotation while attacking (movement damped in controller via AttackLock)

        // Fire attack on Animator (upper-body layer should have the Attack tag)
        playerController.Animator.ResetTrigger("attack");
        playerController.Animator.SetTrigger("attack");
    }

    public override void Tick(float deltaTime)
    {
        // Inspect the upper-body layer: when it's no longer an "Attack" state, we are done.
        int upperBody = playerController.UpperBodyLayerIndex;
        var current = playerController.Animator.GetCurrentAnimatorStateInfo(upperBody);
        var next = playerController.Animator.GetNextAnimatorStateInfo(upperBody);
        bool inTransition = playerController.Animator.IsInTransition(upperBody);

        if (!current.IsTag("Attack") && !next.IsTag("Attack") && !inTransition)
        {
            stateMachine.SwitchState(playerController.LocomotionState);
        }
    }

    public override void Exit()
    {
        // Release movement/rotation lock
        playerController.StateFlags.AttackLock = false;
    }
}
