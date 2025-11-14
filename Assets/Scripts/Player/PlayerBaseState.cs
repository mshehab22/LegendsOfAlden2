// PlayerBaseState: “Common API and shared references for all states.”
/// <summary>
///     Abstract base class all states inherit from.
///     Keeps references to the PlayerController and the StateMachine,
///     and defines the overridable lifecycle hooks.
/// </summary>

public abstract class PlayerBaseState : State
{
    // Instances the states will use:
    protected readonly PlayerController playerController; // gameplay API
    protected readonly PlayerStateMachine stateMachine;   // to request state changes

    protected PlayerBaseState(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }
}
