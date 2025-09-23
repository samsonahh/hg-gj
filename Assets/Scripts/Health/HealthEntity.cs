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

    [Header("Effect Randomization")]
    [Tooltip("Maximum random offset applied to each axis when spawning effects.")]
    [SerializeField] private float effectRandomOffset = 0.15f;

    private int lastSpawnIndex = -1; // Track last used spawn index

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

        // Instantiate damage effect at a random spawn position, with a small random offset
        if (damageEffectPrefab != null && spawnPositions != null && spawnPositions.Count > 0)
        {
            int randomIndex = GetNonRepeatingRandomIndex();
            lastSpawnIndex = randomIndex;

            Vector3 baseWorldPos = transform.TransformPoint(spawnPositions[randomIndex]);

            // Generate a small random offset using Perlin-like smooth randomness
            float timeSeed = Time.time * 10f + UnityEngine.Random.value * 100f;
            float offsetX = (Mathf.PerlinNoise(timeSeed, baseWorldPos.x) - 0.5f) * 2f * effectRandomOffset;
            float offsetY = (Mathf.PerlinNoise(timeSeed + 33.3f, baseWorldPos.y) - 0.5f) * 2f * effectRandomOffset;
            float offsetZ = (Mathf.PerlinNoise(timeSeed + 77.7f, baseWorldPos.z) - 0.5f) * 2f * effectRandomOffset;
            Vector3 randomOffset = new Vector3(offsetX, offsetY, offsetZ);

            Vector3 spawnPos = baseWorldPos + randomOffset;

            GameObject effect = Instantiate(
                damageEffectPrefab,
                spawnPos,
                Quaternion.identity
            );
            Destroy(effect, 1.5f);
        }

        if (currentHealth == 0)
            Die();
    }

    /// <summary>
    /// Returns a random index that is not the same as the last used index.
    /// </summary>
    private int GetNonRepeatingRandomIndex()
    {
        int count = spawnPositions.Count;
        if (count == 1) return 0;
        int index;
        do
        {
            index = UnityEngine.Random.Range(0, count);
        } while (index == lastSpawnIndex);
        return index;
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