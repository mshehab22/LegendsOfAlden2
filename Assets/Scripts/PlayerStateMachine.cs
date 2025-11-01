// PlayerStateMachine: “Tiny orchestrator; guarantees orderly Exit/Enter on transitions.”
using UnityEngine;

/// <summary>
///     Minimal state machine: keeps a pointer to the current state and
///     guarantees the Enter/Exit contract when switching.
/// </summary>

public sealed class PlayerStateMachine
{
    public PlayerBaseState Current { get; private set; }

    public void Initialize(PlayerBaseState startState)
    {
        Current = startState;
        Current.Enter();
    }

    public void ChangeState(PlayerBaseState nextState)
    {
        // Order matters: Exit old -> switch -> Enter new
        Current.Exit();
        Current = nextState;
        Current.Enter();
    }
}
