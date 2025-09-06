using DG.Tweening.Core.Easing;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInputActions;

public class InputManager : Singleton<InputManager>, IPlayerActions, IUIActions
{
    public PlayerInputActions InputActions { get; private set; }

    public Vector2 MoveDirection => InputActions.Player.Move.ReadValue<Vector2>();
    public Vector2 LookDirection => InputActions.Player.Look.ReadValue<Vector2>();
    public event Action Attack = delegate { };

    private protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// Creates a new InputActions instance and sets the callbacks for Player and UI actions.
    /// Needs to be done once in an entire game session.
    /// </summary>
    private void CreatePlayerActions()
    {
        InputActions = new PlayerInputActions();
        InputActions.Player.SetCallbacks(this);
        InputActions.UI.SetCallbacks(this);
    }

    /// <summary>
    /// Prevents all inputs from being processed.
    /// </summary>
    public void DisableAllActions()
    {
        if (InputActions == null)
            CreatePlayerActions();

        InputActions.Disable();
    }

    /// <summary>
    /// Enables the player actions and disables the UI actions.
    /// Locks the cursor accordingly.
    /// </summary>
    public void EnablePlayerActions()
    {
        if (InputActions == null)
            CreatePlayerActions();

        InputActions.Enable();
        InputActions.Player.Enable();
        InputActions.UI.Disable();

        LockCursor(true);
    }

    /// <summary>
    /// Enables the UI actions and disables the player actions.
    /// Unlocks the cursor accordingly.
    /// </summary>
    public void EnableUIActions()
    {
        if (InputActions == null)
            CreatePlayerActions();

        InputActions.Enable();
        InputActions.Player.Disable();
        InputActions.UI.Enable();

        LockCursor(false);
    }

    public void OnLook(InputAction.CallbackContext context)
    {

    }
    public void OnMove(InputAction.CallbackContext context)
    {

    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
            Attack?.Invoke();
    }
    public void OnPause(InputAction.CallbackContext context)
    {

    }
    public void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None; ;
        Cursor.visible = !locked;
    }

    // UI Actions (no need to touch)
    public void OnUnPause(InputAction.CallbackContext context)
    {

    }
    public void OnPoint(InputAction.CallbackContext context)
    {

    }
    public void OnClick(InputAction.CallbackContext context)
    {

    }
    public void OnScrollWheel(InputAction.CallbackContext context)
    {

    }
    public void OnMiddleClick(InputAction.CallbackContext context)
    {

    }
    public void OnRightClick(InputAction.CallbackContext context)
    {

    }
}