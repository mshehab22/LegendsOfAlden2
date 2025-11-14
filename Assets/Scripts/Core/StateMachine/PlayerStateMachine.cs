// PlayerStateMachine: “Tiny orchestrator; guarantees orderly Exit/Enter on transitions.”

using UnityEngine;

/// <summary>
///     Minimal state machine: keeps a pointer to the current state and
///     guarantees the Enter/Exit contract when switching.
/// </summary>
public class PlayerStateMachine : StateMachine
{
    [field: SerializeField] public InputReader InputReader { get; private set; }
    [field: SerializeField] public CharacterController Controller { get; private set; }
    [field: SerializeField] public Animator Animator { get; private set; }
    [field: SerializeField] public float FreeLookMovementSpeed { get; private set; }


    private void Start()
    {
        SwitchState(new PlayerTestState(this));
    }
}
