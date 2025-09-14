using System;
using UnityEngine;

[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField, Min(1)]
    private int maxHealth = 100;

    [SerializeField]
    private int currentHealth;

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
    public void AddHealth(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        // add some UI feedback here
    }
}