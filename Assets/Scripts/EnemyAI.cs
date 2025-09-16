using UnityEngine;
using UnityEngine.AI;

// Credits to "Dave / GameDevelopment" (https://www.youtube.com/watch?v=UjkSFoLxesw)
public enum EnemyState
{
    Idle,
    Walk,
    Shoot,
    Reload
}
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Navigation"), Tooltip("Navigation agent and ground detection settings.")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private LayerMask groundLayer = ~0;

    [Header("Targeting"), Tooltip("How the enemy detects and tracks the player.")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float sightRange = 18f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private bool requireLineOfSight = true;
    [SerializeField] private LayerMask obstacleMask = ~0;
    [SerializeField] private float targetRefreshInterval = 0.75f;

    [Header("Idle Settings"), Tooltip("Idle behavior and timing when not pursuing the player.")]
    [SerializeField] private Vector2 idleTimeRange = new(1.25f, 2.75f);
    [SerializeField] private bool wanderWhenIdle = true; // Toggle: when false, enemy stays stationary in Idle

    [Header("Patrol"), Tooltip("Random patrol movement settings.")]
    [SerializeField] private float walkPointRange = 10f;
    [SerializeField] private float reachedPointDistance = 1.15f;
    [SerializeField] private float pathSampleInterval = 0.25f;

    [Header("Combat"), Tooltip("Projectile and shooting configuration.")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float timeBetweenShots = 0.75f;
    [SerializeField] private int magazineSize = 5;
    [SerializeField] private float projectileSpread = 2f;
    [SerializeField] private float muzzleVerticalOffset = 1.2f;

    [Header("Reload"), Tooltip("Reload timing for ranged attacks.")]
    [SerializeField] private float reloadDuration = 2.0f;

    [Header("Movement"), Tooltip("Movement speeds for chasing and patrolling.")]
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float patrolSpeed = 2.2f;

    [Header("Debug"), Tooltip("Debug visualization and gizmo colors.")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color sightColor = Color.yellow;
    [SerializeField] private Color attackColor = Color.red;
    [SerializeField] private Color walkPointColor = Color.cyan;
    [SerializeField] private Color targetColor = Color.magenta;

    // State
    private EnemyState _state = EnemyState.Idle;

    // Targeting
    private float _targetRefreshTimer;
    private Transform _target;
    private bool _playerInSight;
    private bool _playerInAttack;

    // Timers/Ammo
    private float _idleTimer;
    private float _shotCooldown;
    private float _reloadTimer;
    private int _currentAmmo;

    // Patrol
    private Vector3 _walkPoint;
    private bool _hasWalkPoint;
    private float _pathSampleTimer;

    // Constants / caches
    private const float GROUND_CHECK_DISTANCE = 2f;
    private float _sightRangeSqr;
    private float _attackRangeSqr;
    private float _reachedPointDistanceSqr;

    // Non-alloc target search
    private readonly Collider[] _targetHits = new Collider[16];

    private void Awake()
    {
        agent ??= GetComponent<NavMeshAgent>();
        _currentAmmo = Mathf.Max(0, magazineSize);
        ResetIdleTimer();

        _sightRangeSqr = sightRange * sightRange;
        _attackRangeSqr = attackRange * attackRange;
        _reachedPointDistanceSqr = reachedPointDistance * reachedPointDistance;

        _targetRefreshTimer = 0f;
        _pathSampleTimer = 0f;
    }

    private void Update()
    {
        if (agent == null)
            return;

        RefreshTargetIfNeeded();
        SenseTarget();
        UpdateTimers();
        StateLoop();
    }

    private void RefreshTargetIfNeeded()
    {
        _targetRefreshTimer -= Time.deltaTime;

        if (_target != null && !_target.gameObject.activeInHierarchy)
            _target = null;

        if (_targetRefreshTimer > 0f && _target != null)
            return;

        _targetRefreshTimer = targetRefreshInterval;
        AcquireTarget();
    }

    private void AcquireTarget()
    {
        Transform best = null;
        float bestDistSqr = float.MaxValue;

        // Primary: physics overlap (non-alloc) if a player layer mask is provided
        if (playerLayer.value != 0)
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, sightRange, _targetHits, playerLayer, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var t = _targetHits[i].transform;
                if (t == null) continue;

                float dSqr = (t.position - transform.position).sqrMagnitude;
                if (dSqr < bestDistSqr)
                {
                    bestDistSqr = dSqr;
                    best = t;
                }
            }
        }

        // Fallback: tag search if no layer result
        if (best == null && !string.IsNullOrEmpty(playerTag))
        {
            var tagged = GameObject.FindGameObjectsWithTag(playerTag);
            for (int i = 0; i < tagged.Length; i++)
            {
                var t = tagged[i].transform;
                float dSqr = (t.position - transform.position).sqrMagnitude;
                if (dSqr <= _sightRangeSqr && dSqr < bestDistSqr)
                {
                    bestDistSqr = dSqr;
                    best = t;
                }
            }
        }

        _target = best;
    }

    private void SenseTarget()
    {
        if (_target == null)
        {
            _playerInSight = false;
            _playerInAttack = false;
            return;
        }

        Vector3 toTarget = _target.position - transform.position;
        float distSqr = toTarget.sqrMagnitude;

        bool inSightRadius = distSqr <= _sightRangeSqr;
        bool inAttackRadius = distSqr <= _attackRangeSqr;

        bool hasLOS = true;
        if (requireLineOfSight && inSightRadius)
        {
            Vector3 origin = transform.position + Vector3.up * 1.5f;
            Vector3 targetPos = _target.position + Vector3.up * 1.0f;
            Vector3 dir = (targetPos - origin).normalized;
            hasLOS = !Physics.Raycast(origin, dir, out RaycastHit hit, sightRange, obstacleMask, QueryTriggerInteraction.Ignore)
                     || hit.transform == _target
                     || hit.transform.IsChildOf(_target);
        }

        _playerInSight = inSightRadius && hasLOS;
        _playerInAttack = inAttackRadius && hasLOS;
    }

    private void UpdateTimers()
    {
        if (_shotCooldown > 0f) _shotCooldown = Mathf.Max(0f, _shotCooldown - Time.deltaTime);
        if (_reloadTimer > 0f) _reloadTimer = Mathf.Max(0f, _reloadTimer - Time.deltaTime);
        if (_idleTimer > 0f) _idleTimer = Mathf.Max(0f, _idleTimer - Time.deltaTime);

        _pathSampleTimer += Time.deltaTime;
    }

    private void StateLoop()
    {
        switch (_state)
        {
            case EnemyState.Idle: UpdateIdle(); break;
            case EnemyState.Walk: UpdateWalk(); break;
            case EnemyState.Shoot: UpdateShoot(); break;
            case EnemyState.Reload: UpdateReload(); break;
        }
    }

    private void UpdateIdle()
    {
        agent.ResetPath();

        if (_playerInAttack && _currentAmmo > 0) { ChangeState(EnemyState.Shoot); return; }
        if (_playerInAttack && _currentAmmo <= 0) { BeginReload(); return; }
        if (_playerInSight && !_playerInAttack) { ChangeState(EnemyState.Walk); return; }
        if (_idleTimer <= 0f && wanderWhenIdle) { ChangeState(EnemyState.Walk); }
    }

    private void UpdateWalk()
    {
        agent.speed = _playerInSight ? chaseSpeed : patrolSpeed;

        if (_playerInAttack && _currentAmmo > 0) { ChangeState(EnemyState.Shoot); return; }
        if (_playerInAttack && _currentAmmo <= 0) { BeginReload(); return; }

        if (_playerInSight && !_playerInAttack)
        {
            MoveToTarget();
            return;
        }

        // If we lost sight and wandering is disabled, go back to Idle (stay stationary).
        if (!wanderWhenIdle)
        {
            ResetIdleTimer();
            ChangeState(EnemyState.Idle);
            return;
        }

        Patrol();
    }

    private void UpdateShoot()
    {
        agent.ResetPath();
        FaceTarget();

        if (!_playerInSight) { ChangeState(EnemyState.Walk); return; }
        if (!_playerInAttack && _playerInSight) { ChangeState(EnemyState.Walk); return; }
        if (_currentAmmo <= 0) { BeginReload(); return; }

        TryShoot();
    }

    private void UpdateReload()
    {
        agent.ResetPath();

        if (_reloadTimer > 0f)
            return;

        FinishReload();

        if (_playerInAttack) { ChangeState(EnemyState.Shoot); return; }
        if (_playerInSight) { ChangeState(EnemyState.Walk); return; }

        ResetIdleTimer();
        ChangeState(EnemyState.Idle);
    }

    private void TryShoot()
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

            Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(dir, Vector3.up));
            _currentAmmo--;
            _shotCooldown = timeBetweenShots;
            return;
        }

        // If we can't shoot (missing refs), still set cooldown to avoid spamming logic
        _shotCooldown = timeBetweenShots;
    }

    private void BeginReload()
    {
        if (_state == EnemyState.Reload)
            return;

        _reloadTimer = reloadDuration;
        ChangeState(EnemyState.Reload);
    }

    private void FinishReload()
    {
        _currentAmmo = magazineSize;
    }

    private void ResetIdleTimer()
    {
        _idleTimer = Random.Range(idleTimeRange.x, idleTimeRange.y);
    }

    private void Patrol()
    {
        if (!_hasWalkPoint)
        {
            if (!TryGetRandomWalkPoint(out _walkPoint))
                return;

            _hasWalkPoint = true;
        }

        if (_pathSampleTimer >= pathSampleInterval)
        {
            agent.SetDestination(_walkPoint);
            _pathSampleTimer = 0f;
        }

        if ((transform.position - _walkPoint).sqrMagnitude <= _reachedPointDistanceSqr)
        {
            _hasWalkPoint = false;
            ResetIdleTimer();
            ChangeState(EnemyState.Idle);
        }
    }

    private void MoveToTarget()
    {
        if (_target == null)
            return;

        if (_pathSampleTimer < pathSampleInterval)
            return;

        agent.SetDestination(_target.position);
        _pathSampleTimer = 0f;
    }

    private bool TryGetRandomWalkPoint(out Vector3 result)
    {
        for (int i = 0; i < 12; i++)
        {
            float randZ = Random.Range(-walkPointRange, walkPointRange);
            float randX = Random.Range(-walkPointRange, walkPointRange);
            Vector3 candidate = new(transform.position.x + randX, transform.position.y, transform.position.z + randZ);

            // Grounded and on NavMesh
            if (!Physics.Raycast(candidate + Vector3.up * 2f, Vector3.down, out RaycastHit hit, GROUND_CHECK_DISTANCE + 2f, groundLayer))
                continue;

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit navHit, 1.25f, NavMesh.AllAreas))
                continue;

            result = navHit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    private void FaceTarget()
    {
        if (_target == null)
            return;

        Vector3 dir = _target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.0001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
    }

    private void ChangeState(EnemyState newState)
    {
        if (_state == newState)
            return;

        _state = newState;

        switch (_state)
        {
            case EnemyState.Idle:
            case EnemyState.Shoot:
            case EnemyState.Reload:
                agent.ResetPath();
                break;

            case EnemyState.Walk:
                _hasWalkPoint = false; // reacquire as needed
                break;
        }
    }

    // Public helpers
    public void ForceReload()
    {
        if (_state != EnemyState.Reload)
            BeginReload();
    }

    public EnemyState CurrentState => _state;
    public int CurrentAmmo => _currentAmmo;
    public int MagazineSize => magazineSize;
    public Transform CurrentTarget => _target;

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos)
            return;

        Gizmos.color = sightColor;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Gizmos.color = attackColor;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (_hasWalkPoint)
        {
            Gizmos.color = walkPointColor;
            Gizmos.DrawSphere(_walkPoint, 0.25f);
            Gizmos.DrawLine(transform.position, _walkPoint);
        }

        if (_target != null)
        {
            Gizmos.color = targetColor;
            Gizmos.DrawLine(transform.position + Vector3.up * 1.2f, _target.position + Vector3.up * 1.2f);
        }

        if (firePoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.forward * 2f);
        }
    }
}