public abstract class State
{
    public abstract void Enter(); // Called once on entry to the state (set flags, start animations, etc.)
    public abstract void Tick(float deltaTime);
    public abstract void Exit(); // Called once on leaving the state (clear flags, stop effects, etc.)
}
