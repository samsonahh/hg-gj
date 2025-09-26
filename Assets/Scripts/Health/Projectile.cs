using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 20f; // Transform speed
    [SerializeField] private float decayTime = 3f; // Decay time in seconds
    [SerializeField] private int damage = 10;

    public void SetDamage(int value) => damage = value;

    private void Start()
    {
        Destroy(gameObject, decayTime);
    }

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Try Health component
        Health health = collision.gameObject.GetComponent<Health>();
        if (health != null && health.gameObject != null && health.gameObject.activeInHierarchy)
        {
            health.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Try HealthEntity component
        HealthEntity healthEntity = collision.gameObject.GetComponent<HealthEntity>();
        if (healthEntity != null && healthEntity.gameObject != null && healthEntity.gameObject.activeInHierarchy)
        {
            healthEntity.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ParryWindow"))
            return;
        // Try Health component
        Health health = other.gameObject.GetComponent<Health>();
        if (health != null && health.gameObject != null && health.gameObject.activeInHierarchy)
        {
            health.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Try HealthEntity component
        HealthEntity healthEntity = other.gameObject.GetComponent<HealthEntity>();
        if (healthEntity != null && healthEntity.gameObject != null && healthEntity.gameObject.activeInHierarchy)
        {
            healthEntity.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
        Destroy(gameObject);
    }
}