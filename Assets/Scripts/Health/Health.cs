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
    public event Action<int, int> OnHealed = delegate { }; // NEW: fired when health increases

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

        int before = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        if (currentHealth != before)
        {
            OnHealthChanged.Invoke(currentHealth, maxHealth);
            OnHealed.Invoke(currentHealth, maxHealth); // notify heals explicitly
        }
    }

    /// <summary>
    /// Reset health to max.
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged.Invoke(currentHealth, maxHealth);
        OnHealed.Invoke(currentHealth, maxHealth);
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
        GameManager.Instance.ChangeState(GameState.Results);
        Destroy(gameObject);
    }

    /// <summary>
    /// Adds health and notifies listeners. Returns true if health increased.
    /// Use this when pickups add health so UI/effects can react.
    /// </summary>
    public bool AddHealth(int amount)
    {
        if (amount <= 0 || currentHealth <= 0)
            return false;

        int before = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        if (currentHealth != before)
        {
            OnHealthChanged.Invoke(currentHealth, maxHealth);
            OnHealed.Invoke(currentHealth, maxHealth); // explicit heal event
        }

        return currentHealth > before;
    }
}