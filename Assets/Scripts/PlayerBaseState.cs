// PlayerBaseState: “Common API and shared references for all states.”
using UnityEngine;

/// <summary>
///     Abstract base class all states inherit from.
///     Keeps references to the PlayerController and the StateMachine,
///     and defines the overridable lifecycle hooks.
/// </summary>

public abstract class PlayerBaseState
{
    // Instances the states will use:
    protected readonly PlayerController playerController; // gameplay API
    protected readonly PlayerStateMachine stateMachine;   // to request state changes

    protected PlayerBaseState(PlayerController playerController, PlayerStateMachine stateMachine)
    {
        this.playerController = playerController;
        this.stateMachine = stateMachine;
    }
  
    public virtual void Enter() { } // Called once on entry to the state (set flags, start animations, etc.)
    public virtual void Exit() { }  // Called once on leaving the state (clear flags, stop effects, etc.)
    public virtual void HandleInput() { }  // Called each frame before logic (react to input: decide whether to change state)
    public virtual void LogicUpdate() { } // Called each frame to perform the state's ongoing behavior
}
