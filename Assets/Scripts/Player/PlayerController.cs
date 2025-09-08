using NaughtyAttributes;
using UnityEngine;
using PlayerStates;

public class PlayerController : MonoBehaviour
{
    public InputManager Input { get; private set; } // for easier access

    [field: Header("References")]
    [field: SerializeField] public CharacterController Controller;

    private StateMachine<PlayerController> stateMachine;
    [field: Header("States")]
    [field: SerializeField] public GroundedSuperState GroundedSuperState { get; private set; } = new();
    [field: SerializeField] public AirborneSuperState AirborneSuperState { get; private set; } = new();
    [field: SerializeField] public WallRunState WallRunState { get; private set; } = new();

    public bool IsGrounded => GroundedSuperState.IsGrounded; // for easier access
    private Vector3 velocity;
    [ShowNativeProperty] public Vector3 Velocity => velocity;
    public Vector3 PlanarVelocity => velocity.WithY(0f);

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
    
    public void SetVelocity(Vector3 newVelocity) => velocity = newVelocity;
    public void SetPlanarVelocity(Vector3 newVelocity) => velocity = new Vector3(newVelocity.x, velocity.y, newVelocity.z);
    
    public void ApplyPlanarVelocity() => Controller.Move(Time.deltaTime * velocity.WithY(0));

    public void ApplyGravity()
    {
        velocity += Time.deltaTime * Physics.gravity;
        
        Controller.Move(velocity.y * Time.deltaTime * Vector3.down);
    }
}