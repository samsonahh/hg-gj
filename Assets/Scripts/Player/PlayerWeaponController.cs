using System;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    public event Action<int> OnWeaponSwitchRequested = delegate { };

    private Arsenal arsenal;

    private void Awake()
    {
        
        arsenal = GetComponentInChildren<Arsenal>(includeInactive: true);
        if (arsenal != null)
            OnWeaponSwitchRequested += arsenal.SwitchWeapon;
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.Scroll += OnScroll;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.Scroll -= OnScroll;

        if (arsenal != null)
            OnWeaponSwitchRequested -= arsenal.SwitchWeapon;
    }

    /// <summary>
    /// Handles scroll input and requests a weapon switch.
    /// </summary>
    private void OnScroll(Vector2 scrollDelta)
    {
        int delta = 0;
        if (scrollDelta.y > 0)
            delta = 1;
        else if (scrollDelta.y < 0)
            delta = -1;

        if (delta != 0)
            OnWeaponSwitchRequested.Invoke(delta);
    }
}