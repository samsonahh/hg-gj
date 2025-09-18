using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyAIBase : MonoBehaviour
{
    [Header("Navigation"), Tooltip("Navigation agent and ground detection settings.")]
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected LayerMask groundLayer = ~0;

    [Header("Targeting"), Tooltip("How the enemy detects and tracks the player.")]
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected string playerTag = "Player";
    [SerializeField] protected float sightRange = 18f;
    [SerializeField] protected float attackRange = 10f;
    [SerializeField] protected bool requireLineOfSight = true;
    [SerializeField] protected LayerMask obstacleMask = ~0;
    [SerializeField] protected float targetRefreshInterval = 0.75f;

    [Header("Idle Settings"), Tooltip("Idle behavior and timing when not pursuing the player.")]
    [SerializeField] protected Vector2 idleTimeRange = new(1.25f, 2.75f);
    [SerializeField] protected bool wanderWhenIdle = true;

    [Header("Patrol"), Tooltip("Random patrol movement settings.")]
    [SerializeField] protected float walkPointRange = 10f;
    [SerializeField] protected float reachedPointDistance = 1.15f;
    [SerializeField] protected float pathSampleInterval = 0.25f;

    [Header("Combat"), Tooltip("Projectile and shooting configuration.")]
    [SerializeField] protected GameObject projectilePrefab;
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected float timeBetweenShots = 0.75f;
    [SerializeField] protected int magazineSize = 5;
    [SerializeField] protected float projectileSpread = 2f;
    [SerializeField] protected float muzzleVerticalOffset = 1.2f;
    [SerializeField] protected int projectileDamage = 10; // New: control projectile damage

    [Header("Reload"), Tooltip("Reload timing for ranged attacks.")]
    [SerializeField] protected float reloadDuration = 2.0f;

    [Header("Movement"), Tooltip("Movement speeds for chasing and patrolling.")]
    [SerializeField] protected float chaseSpeed = 4f;
    [SerializeField] protected float patrolSpeed = 2.2f;

    [Header("Debug"), Tooltip("Debug visualization and gizmo colors.")]
    [SerializeField] protected bool showGizmos = true;
    [SerializeField] protected Color sightColor = Color.yellow;
    [SerializeField] protected Color attackColor = Color.red;
    [SerializeField] protected Color walkPointColor = Color.cyan;
    [SerializeField] protected Color targetColor = Color.magenta;

    protected EnemyState _state = EnemyState.Idle;
    protected float _targetRefreshTimer;
    protected Transform _target;
    protected bool _playerInSight;
    protected bool _playerInAttack;
    protected float _idleTimer;
    protected float _shotCooldown;
    protected float _reloadTimer;
    protected int _currentAmmo;
    protected Vector3 _walkPoint;
    protected bool _hasWalkPoint;
    protected float _pathSampleTimer;
    protected const float GROUND_CHECK_DISTANCE = 2f;
    protected float _sightRangeSqr;
    protected float _attackRangeSqr;
    protected float _reachedPointDistanceSqr;
    protected readonly Collider[] _targetHits = new Collider[16];

    protected virtual void Awake()
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

    protected virtual void Update()
    {
        if (agent == null)
            return;

        RefreshTargetIfNeeded();
        SenseTarget();
        UpdateTimers();
        StateLoop();
    }

    protected virtual void RefreshTargetIfNeeded()
    {
        _targetRefreshTimer -= Time.deltaTime;

        if (_target != null && !_target.gameObject.activeInHierarchy)
            _target = null;

        if (_targetRefreshTimer > 0f && _target != null)
            return;

        _targetRefreshTimer = targetRefreshInterval;
        AcquireTarget();
    }

    protected virtual void AcquireTarget()
    {
        Transform best = null;
        float bestDistSqr = float.MaxValue;

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

    protected virtual void SenseTarget()
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

    protected virtual void UpdateTimers()
    {
        if (_shotCooldown > 0f) _shotCooldown = Mathf.Max(0f, _shotCooldown - Time.deltaTime);
        if (_reloadTimer > 0f) _reloadTimer = Mathf.Max(0f, _reloadTimer - Time.deltaTime);
        if (_idleTimer > 0f) _idleTimer = Mathf.Max(0f, _idleTimer - Time.deltaTime);

        _pathSampleTimer += Time.deltaTime;
    }

    protected virtual void StateLoop()
    {
        switch (_state)
        {
            case EnemyState.Idle: UpdateIdle(); break;
            case EnemyState.Walk: UpdateWalk(); break;
            case EnemyState.Shoot: UpdateShoot(); break;
            case EnemyState.Reload: UpdateReload(); break;
        }
    }

    protected virtual void UpdateIdle()
    {
        agent.ResetPath();

        if (_playerInAttack && _currentAmmo > 0) { ChangeState(EnemyState.Shoot); return; }
        if (_playerInAttack && _currentAmmo <= 0) { BeginReload(); return; }
        if (_playerInSight && !_playerInAttack) { ChangeState(EnemyState.Walk); return; }
        if (_idleTimer <= 0f && wanderWhenIdle) { ChangeState(EnemyState.Walk); }
    }

    protected virtual void UpdateWalk()
    {
        agent.speed = _playerInSight ? chaseSpeed : patrolSpeed;

        if (_playerInAttack && _currentAmmo > 0) { ChangeState(EnemyState.Shoot); return; }
        if (_playerInAttack && _currentAmmo <= 0) { BeginReload(); return; }

        if (_playerInSight && !_playerInAttack)
        {
            MoveToTarget();
            return;
        }

        if (!wanderWhenIdle)
        {
            ResetIdleTimer();
            ChangeState(EnemyState.Idle);
            return;
        }

        Patrol();
    }

    protected virtual void UpdateShoot()
    {
        agent.ResetPath();
        FaceTarget();

        if (!_playerInSight) { ChangeState(EnemyState.Walk); return; }
        if (!_playerInAttack && _playerInSight) { ChangeState(EnemyState.Walk); return; }
        if (_currentAmmo <= 0) { BeginReload(); return; }

        TryShoot();
    }

    protected virtual void UpdateReload()
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

    protected virtual void TryShoot()
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
                proj.SetDamage(projectileDamage); // Set projectile damage

            _currentAmmo--;
            _shotCooldown = timeBetweenShots;
            return;
        }

        _shotCooldown = timeBetweenShots;
    }

    protected virtual void BeginReload()
    {
        if (_state == EnemyState.Reload)
            return;

        _reloadTimer = reloadDuration;
        ChangeState(EnemyState.Reload);
    }

    protected virtual void FinishReload()
    {
        _currentAmmo = magazineSize;
    }

    protected virtual void ResetIdleTimer()
    {
        _idleTimer = Random.Range(idleTimeRange.x, idleTimeRange.y);
    }

    protected virtual void Patrol()
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

        // --- Fix: If agent is stuck or can't reach the walk point, reset walk point and go idle ---
        if (agent.pathPending == false)
        {
            // If agent can't reach the point or is stuck
            if (agent.pathStatus == NavMeshPathStatus.PathPartial ||
                agent.pathStatus == NavMeshPathStatus.PathInvalid ||
                (agent.remainingDistance > 0.1f && agent.velocity.sqrMagnitude < 0.01f))
            {
                _hasWalkPoint = false;
                ResetIdleTimer();
                ChangeState(EnemyState.Idle);
                return;
            }
        }

        if ((transform.position - _walkPoint).sqrMagnitude <= _reachedPointDistanceSqr)
        {
            _hasWalkPoint = false;
            ResetIdleTimer();
            ChangeState(EnemyState.Idle);
        }
    }

    protected virtual void MoveToTarget()
    {
        if (_target == null)
            return;

        if (_pathSampleTimer < pathSampleInterval)
            return;

        agent.SetDestination(_target.position);
        _pathSampleTimer = 0f;
    }

    protected virtual bool TryGetRandomWalkPoint(out Vector3 result)
    {
        for (int i = 0; i < 12; i++)
        {
            float randZ = Random.Range(-walkPointRange, walkPointRange);
            float randX = Random.Range(-walkPointRange, walkPointRange);
            Vector3 candidate = new(transform.position.x + randX, transform.position.y, transform.position.z + randZ);

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

    protected virtual void FaceTarget()
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

    protected virtual void ChangeState(EnemyState newState)
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
                _hasWalkPoint = false;
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
}