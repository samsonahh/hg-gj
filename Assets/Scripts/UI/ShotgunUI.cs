using TMPro;
using UnityEngine;

public class ShotgunUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI ammoText;

    private Shotgun shotgun;

    private void OnEnable()
    {
        // Delay subscription until end of frame to ensure Shotgun is spawned
        StartCoroutine(SubscribeWhenReady());
    }

    private void OnDisable()
    {
        if (shotgun != null)
            shotgun.OnAmmoChanged -= UpdateAmmoUI;
    }

    private System.Collections.IEnumerator SubscribeWhenReady()
    {
        // Wait until end of frame to ensure Bootstrap/Shotgun is loaded
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
        if (ammoText != null)
            ammoText.text = $"Ammo: {current} / {max}";
    }
}