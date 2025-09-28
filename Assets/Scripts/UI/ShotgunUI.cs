using TMPro;
using UnityEngine;

public class ShotgunUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private ShotgunState shotgunState;

    private void OnEnable()
    {
        if (shotgunState != null)
        {
            shotgunState.OnAmmoChanged += UpdateAmmoUI;
            UpdateAmmoUI(shotgunState.CurrentAmmo, shotgunState.MaxAmmo);
        }
    }

    private void OnDisable()
    {
        if (shotgunState != null)
            shotgunState.OnAmmoChanged -= UpdateAmmoUI;
    }

    private void UpdateAmmoUI(int current, int max)
    {
        if (shotgunState != null && shotgunState.InfiniteAmmo)
        {
            if (ammoText != null)
                ammoText.text = $"Ammo: \u221E";
        }
        else
        {
            if (ammoText != null)
                ammoText.text = $"Ammo: {current} / {max}";
        }
    }
}