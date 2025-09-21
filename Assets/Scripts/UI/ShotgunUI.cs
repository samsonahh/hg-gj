using TMPro;
using UnityEngine;

public class ShotgunUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Shotgun shotgun;

    private void OnEnable()
    {
        shotgun.OnAmmoChanged += UpdateAmmoUI;
        UpdateAmmoUI(shotgun.CurrentAmmo, shotgun.MaxAmmo);
    }

    private void OnDisable()
    {
        if (shotgun != null)
            shotgun.OnAmmoChanged -= UpdateAmmoUI;
    }

    private void UpdateAmmoUI(int current, int max)
    {
        if (shotgun != null && shotgun.InfiniteAmmo)
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