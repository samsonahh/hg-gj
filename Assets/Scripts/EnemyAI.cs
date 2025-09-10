using UnityEngine;
using UnityEngine.AI;

// Credits to "Dave / GameDevelopment" (https://www.youtube.com/watch?v=UjkSFoLxesw)

public enum EnemyAIState
{
    Patrolling,
    Chasing,
    Attacking    
}

public class EnemyAI : MonoBehaviour
{
    [Header("General Settings")]
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask groundLayer;
    public LayerMask playerLayer;

    [Header("Patrol Settings")]
    public Vector3 walkPoint;
    public float walkPointRange;
    bool _walkPointSet;

    [Header("Attack Settings")]
    public float timeBetweenAttacks;
    bool _hasAlreadyAttacked;

    [Header("State Settings")]
    public float sightRange;
    public float attackRange;
    bool _isPlayerInSightRange;
    bool _isPlayerInAttackRange;
    [SerializeField] EnemyAIState _currentState;

    // dist = Shorthand for "distance"
    float _DIST_FROM_GROUND = 2f;
    float _REACHED_WALK_POINT_DIST = 1f;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        SettingState();

        switch (_currentState)
        {
            case EnemyAIState.Patrolling:
                Patroling();
                break;
            case EnemyAIState.Chasing:
                ChasePlayer();
                break;
            case EnemyAIState.Attacking:
                AttackPlayer();
                break;
            default:
                Patroling();
                break;
        }
        }

    void SettingState()
    {
        _isPlayerInSightRange = Physics.CheckSphere(transform.position, sightRange, playerLayer);
        _isPlayerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerLayer);

        if (!_isPlayerInAttackRange && !_isPlayerInSightRange) _currentState = EnemyAIState.Patrolling;
        if (!_isPlayerInAttackRange && _isPlayerInSightRange) _currentState = EnemyAIState.Chasing;
        if (_isPlayerInAttackRange && _isPlayerInSightRange) _currentState = EnemyAIState.Attacking;
    }

    void Patroling()
    {
        if (!_walkPointSet) SearchWalkPoint();
        if (_walkPointSet) agent.SetDestination(walkPoint);

        Vector3 distToWalkPoint = transform.position - walkPoint;
        if (distToWalkPoint.magnitude < _REACHED_WALK_POINT_DIST) _walkPointSet = false;
    }

    void SearchWalkPoint()
    {
        float randZ = Random.Range(-walkPointRange, walkPointRange);
        float randX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randX, transform.position.y, transform.position.z + randZ);

        if (Physics.Raycast(walkPoint, -transform.up, _DIST_FROM_GROUND, groundLayer)) _walkPointSet = true;
    }

    void ChasePlayer() => agent.SetDestination(player.position);

    void AttackPlayer()
    {
        if(!_hasAlreadyAttacked)
        {
            // Attack Code Will Go Here
            Debug.Log("Attacking Player");

            _hasAlreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    void ResetAttack() => _hasAlreadyAttacked = false;

    private void OnDrawGizmos()
    {
        // Attack Sphere
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Sight Sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
