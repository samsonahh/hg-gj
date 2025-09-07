using UnityEngine;
using PlayerStates;

public class PlayerController : MonoBehaviour
{
    public InputManager Input { get; private set; } // for easier access

    [field: Header("References")]
    [field: SerializeField] public Rigidbody RigidBody;
    [field: SerializeField] public CapsuleCollider CapsuleCollider;

    private StateMachine<PlayerController> stateMachine;
    [field: Header("States")]
    [field: SerializeField] public GroundedSuperState GroundedSuperState { get; private set; } = new();
    [field: SerializeField] public AirborneSuperState AirborneSuperState { get; private set; } = new();
    [field: SerializeField] public WallRunState WallRunState { get; private set; } = new();

    public bool IsGrounded => GroundedSuperState.IsGrounded; // for easier access

    private void Awake()
    {
        Input = InputManager.Instance;

        InitializeStateMachine();
    }

    private void InitializeStateMachine()
    {
        stateMachine = new StateMachine<PlayerController>(this);

        GroundedSuperState.Init(stateMachine, this);
        AirborneSuperState.Init(stateMachine, this);
        WallRunState.Init(stateMachine, this);

        stateMachine.ChangeState(GroundedSuperState, true);
    }

    private void OnDestroy()
    {
        stateMachine.Destroy();
    }

    private void OnDrawGizmos()
    {
        if(GroundedSuperState != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + GroundedSuperState.GroundCheckDistance * Vector3.down, GroundedSuperState.GroundCheckRadius);
        }
    }

    private void Update()
    {
        GroundedSuperState.CheckGrounded();

        stateMachine.Update();
    }

    private void FixedUpdate()
    {
        stateMachine.FixedUpdate();
    }
}