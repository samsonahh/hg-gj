using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class HealthEntity : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField, Min(1)]
    private int maxHealth = 100;

    [SerializeField]
    private int currentHealth;

    [Header("Damage Effect")]
    [SerializeField] private GameObject damageEffectPrefab;

    [Header("Spawn Positions")]
    [Tooltip("Local positions around this entity where effects or objects can spawn.")]
    [SerializeField] private List<Vector3> spawnPositions = new List<Vector3>();

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    public event Action<int, int> OnHealthChanged = delegate { };
    public event Action OnDied = delegate { };

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Apply damage to this entity.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHealth <= 0)
            return;

        currentHealth = Mathf.Max(currentHealth - amount, 0);
        OnHealthChanged.Invoke(currentHealth, maxHealth);

        // Instantiate damage effect at a random spawn position, not parented to this entity
        if (damageEffectPrefab != null && spawnPositions != null && spawnPositions.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, spawnPositions.Count);
            Vector3 worldPos = transform.TransformPoint(spawnPositions[randomIndex]);
            GameObject effect = Instantiate(
                damageEffectPrefab,
                worldPos,
                Quaternion.identity
            );
            Destroy(effect, 1.5f);
        }

        if (currentHealth == 0)
            Die();
    }

    /// <summary>
    /// Heal this entity.
    /// </summary>
    public void Heal(int amount)
    {
        if (amount <= 0 || currentHealth <= 0)
            return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Reset health to max.
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Instantly kill this entity.
    /// </summary>
    public void Kill()
    {
        if (currentHealth > 0)
        {
            currentHealth = 0;
            OnHealthChanged.Invoke(currentHealth, maxHealth);
            Die();
        }
    }

    private void Die()
    {
        OnDied.Invoke();
        Destroy(gameObject);
    }

    /// <summary>
    /// Draws gizmos and visualizes spawn positions using the specified prefab.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (spawnPositions == null)
            return;

        Gizmos.color = Color.cyan;
        foreach (var localPos in spawnPositions)
        {
            Vector3 worldPos = transform.TransformPoint(localPos);
            Gizmos.DrawSphere(worldPos, 0.15f);


        }
    }
}