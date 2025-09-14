using UnityEngine;

public class AmmoPickup : PickupBase
{
    private Shotgun shotgun;

    private void Awake()
    {
        shotgun = Object.FindFirstObjectByType<Shotgun>();
    }

    protected override bool CanPickup(Collider other)
    {
        return other.CompareTag("Player") && shotgun != null;
    }

    protected override void OnPickup(Collider other)
    {
        if (shotgun != null)
        {
            shotgun.Reload();
        }
    }
}