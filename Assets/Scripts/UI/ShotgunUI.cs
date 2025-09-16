using TMPro;
using UnityEngine;

public class ShotgunUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI ammoText;

    private Shotgun shotgun;

    private void OnEnable()
    {
        StartCoroutine(SubscribeWhenReady());
    }

    private void OnDisable()
    {
        if (shotgun != null)
            shotgun.OnAmmoChanged -= UpdateAmmoUI;
    }

    private System.Collections.IEnumerator SubscribeWhenReady()
    {
        yield return new WaitForEndOfFrame();

        shotgun = GetComponentInParent<Shotgun>();
        if (shotgun == null)
            shotgun = Object.FindFirstObjectByType<Shotgun>();

        if (shotgun != null)
        {
            shotgun.OnAmmoChanged += UpdateAmmoUI;
            UpdateAmmoUI(shotgun.CurrentAmmo, shotgun.MaxAmmo);
        }
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