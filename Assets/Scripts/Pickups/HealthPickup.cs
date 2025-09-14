using UnityEngine;

public class HealthPickup : PickupBase
{
    [SerializeField] private int healthAmount = 25;

    protected override bool CanPickup(Collider other)
    {
        return other.GetComponent<Health>() != null;
    }

    protected override void OnPickup(Collider other)
    {
        var health = other.GetComponent<Health>();
        if (health != null)
        {
            health.AddHealth(healthAmount);
        }
    }
}