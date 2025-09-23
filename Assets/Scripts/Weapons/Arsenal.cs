using UnityEngine;
using System.Collections.Generic;

public class Arsenal : MonoBehaviour
{
    [SerializeField] private List<WeaponState> weapons = new List<WeaponState>();
    private int currentWeaponIndex = 0;
    public WeaponState CurrentWeapon { get; private set; }

    private void Start()
    {
        SwitchWeapon(0);
    }

    public void SwitchWeapon(int delta)
    {
        if (CurrentWeapon != null)
            CurrentWeapon.Exit();

        currentWeaponIndex = (currentWeaponIndex + delta + weapons.Count) % weapons.Count;
        CurrentWeapon = weapons[currentWeaponIndex];
        CurrentWeapon.Enter();
    }
}