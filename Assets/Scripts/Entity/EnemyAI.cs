using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : EnemyAIBase
{
    [Header("Combat Spread"), Tooltip("Random inaccuracy/spread for projectiles.")]
    [SerializeField] private float projectileSpread = 2f;

    protected override void TryShoot()
    {
        if (_shotCooldown > 0f)
            return;

        if (firePoint != null && projectilePrefab != null && _target != null)
        {
            Vector3 aimPos = _target.position + Vector3.up * muzzleVerticalOffset;
            Vector3 dir = (aimPos - firePoint.position).normalized;

            dir = Quaternion.Euler(
                Random.Range(-projectileSpread, projectileSpread),
                Random.Range(-projectileSpread, projectileSpread),
                0f) * dir;

            var projObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(dir, Vector3.up));
            var proj = projObj.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.SetDamage(projectileDamage);
                proj.SetSpeed(projectileSpeed);
            }

            _currentAmmo--;
            _shotCooldown = timeBetweenShots;
            return;
        }

        _shotCooldown = timeBetweenShots;
    }
}