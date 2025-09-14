using System.Collections.Generic;
using UnityEngine;

public class Arsenal : MonoBehaviour
{
    [Header("Weapon List Config")]
    [SerializeField] private List<GameObject> weapons = new List<GameObject>();

    private int currentWeaponIndex = 0;

    private void OnEnable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.Scroll += OnScroll;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.Scroll -= OnScroll;
    }

    private void Start()
    {
        ActivateWeapon(currentWeaponIndex);
    }

    /// <summary>
    /// Handles scroll input to swap weapons.
    /// </summary>
    private void OnScroll(Vector2 scrollDelta)
    {
        if (weapons.Count == 0)
            return;

        if (scrollDelta.y > 0)
            currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Count;
        else if (scrollDelta.y < 0)
            currentWeaponIndex = (currentWeaponIndex - 1 + weapons.Count) % weapons.Count;
        else
            return;

        ActivateWeapon(currentWeaponIndex);
    }

    /// <summary>
    /// Activates the weapon at the given index and deactivates others.
    /// </summary>
    private void ActivateWeapon(int index)
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i] != null)
                weapons[i].SetActive(i == index);
        }
    }
}