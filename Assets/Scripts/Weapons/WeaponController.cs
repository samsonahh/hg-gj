using UnityEngine;

public class WeaponController : MonoBehaviour
{
    private Arsenal arsenal;

    private void Awake()
    {
        arsenal = GetComponentInChildren<Arsenal>(includeInactive: true);
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.Shoot += OnShoot;
            InputManager.Instance.RightClick += OnParry;
            InputManager.Instance.Jump += OnJump;
            InputManager.Instance.Scroll += OnScroll;
            // Add more as needed (e.g., reload)
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.Shoot -= OnShoot;
            InputManager.Instance.RightClick -= OnParry;
            InputManager.Instance.Jump -= OnJump;
            InputManager.Instance.Scroll -= OnScroll;
            // Remove more as needed
        }
    }

    private void OnShoot() => arsenal?.CurrentWeapon?.OnShoot();
    private void OnParry() => arsenal?.CurrentWeapon?.OnParry();
    private void OnJump() => arsenal?.CurrentWeapon?.OnJump();
    private void OnReload() => arsenal?.CurrentWeapon?.OnReload();
    private void OnWalk() => arsenal?.CurrentWeapon?.OnWalk();

    private void OnScroll(Vector2 scrollDelta)
    {
        int delta = 0;
        if (scrollDelta.y > 0) delta = 1;
        else if (scrollDelta.y < 0) delta = -1;
        if (delta != 0) arsenal?.SwitchWeapon(delta);
    }
}