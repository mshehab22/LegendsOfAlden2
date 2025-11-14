using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, PlayerControls.IGameplayActions
{
    public Vector2 MovementValue { get; private set; }
    public event Action JumpEvent;
    public event Action DodgeEvent;
    public event Action MoveEvent;


    private PlayerControls controls;

    private void Start()
    {
        controls = new PlayerControls();
        controls.Gameplay.SetCallbacks(this);

        controls.Gameplay.Enable();
    }

    private void OnDestroy()
    {
        controls.Gameplay.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MovementValue = context.ReadValue<Vector2>();
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        JumpEvent?.Invoke();
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        DodgeEvent?.Invoke();
    }
}
