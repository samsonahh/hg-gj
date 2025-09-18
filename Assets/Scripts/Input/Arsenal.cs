using System;
using System.Collections.Generic;
using UnityEngine;

public class Arsenal : MonoBehaviour
{
    [Header("Weapon List Config")]
    [SerializeField] private List<GameObject> weapons = new List<GameObject>();

    private int currentWeaponIndex = 0;

    public int CurrentWeaponIndex => currentWeaponIndex;
    public int WeaponCount => weapons.Count;

    public event Action<int> OnWeaponChanged = delegate { };

    private void OnEnable()
    {
        // Subscribe to PlayerWeaponController's event if present
        var controller = GetComponent<PlayerWeaponController>();
        if (controller != null)
            controller.OnWeaponSwitchRequested += SwitchWeapon;
    }

    private void OnDisable()
    {
        var controller = GetComponent<PlayerWeaponController>();
        if (controller != null)
            controller.OnWeaponSwitchRequested -= SwitchWeapon;
    }

    private void Start()
    {
        ActivateWeapon(currentWeaponIndex);
    }

    /// <summary>
    /// Switches weapon by index delta (e.g. +1 or -1).
    /// </summary>
    public void SwitchWeapon(int delta)
    {
        if (weapons.Count == 0 || delta == 0)
            return;

        int newIndex = (currentWeaponIndex + delta + weapons.Count) % weapons.Count;
        if (newIndex != currentWeaponIndex)
        {
            currentWeaponIndex = newIndex;
            ActivateWeapon(currentWeaponIndex);
            OnWeaponChanged.Invoke(currentWeaponIndex);
        }
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