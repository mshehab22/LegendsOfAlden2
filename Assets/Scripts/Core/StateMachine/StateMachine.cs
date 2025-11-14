using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{
    private State currentState;

    private void Update()
    {
        currentState?.Tick(Time.deltaTime);
    }

    public void SwitchState(State nextState)
    {
        // Order matters: Exit old -> switch -> Enter new
        currentState?.Exit();
        currentState = nextState;
        currentState?.Enter();
    }
}
