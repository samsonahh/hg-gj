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
        return other.CompareTag("Player");
    }

    protected override void OnPickup(Collider other)
    {
        // Try to find the Shotgun in the scene, even if it was not found in Awake
        if (shotgun == null || !shotgun.gameObject.activeInHierarchy)
            shotgun = Object.FindFirstObjectByType<Shotgun>(UnityEngine.FindObjectsInactive.Include);

        if (shotgun != null)
        {
            shotgun.Reload();
        }
    }
}