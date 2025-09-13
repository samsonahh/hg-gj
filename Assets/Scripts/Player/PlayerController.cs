using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using PlayerStates;

public class PlayerController : MonoBehaviour
{
    public InputManager Input { get; private set; } // for easier access

    [field: Header("References")]
    [field: SerializeField] public CharacterController Controller { get; private set; }
    [SerializeField] private Transform cameraTarget;

    [Header("Config")] 
    [SerializeField] private float strictSpeedCap = 100f;
    
    private StateMachine<PlayerController> stateMachine;
    [field: Header("States")]
    [field: SerializeField] public GroundedSuperState GroundedSuperState { get; private set; } = new();
    [field: SerializeField] public AirborneSuperState AirborneSuperState { get; private set; } = new();
    [field: SerializeField] public WallRunState WallRunState { get; private set; } = new();

    [ShowNativeProperty] public bool IsGrounded => GroundedSuperState.IsGrounded; // for easier access
    private Vector3 velocity;
    [ShowNativeProperty] public Vector3 Velocity => velocity;
    public Vector3 PlanarVelocity => velocity.WithY(0f);
    [ShowNativeProperty] private float currentSpeed => PlanarVelocity.magnitude;
    
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

        if (cameraTarget != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(cameraTarget.position, 0.1f);
        }
    }

    private void Update()
    {
        GroundedSuperState.CheckGrounded();

        stateMachine.Update();
        
        // Strict cap
        SetPlanarVelocity(Vector3.ClampMagnitude(PlanarVelocity, strictSpeedCap));
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
        if(!IsGrounded)
            velocity += Time.deltaTime * Physics.gravity;
        
        Controller.Move(velocity.y * Time.deltaTime * Vector3.up);
    }

    public void ChangeControllerHeight(float newHeight)
    {
        Controller.height = newHeight;
        Controller.center = Controller.center.WithY(newHeight / 2f);
    }
    
    public void ChangeControllerHeight(float newHeight, float cameraShiftDuration, Ease cameraShiftEaseType)
    {
        ChangeControllerHeight(newHeight);
        
        cameraTarget.DOKill();
        cameraTarget.DOLocalMoveY(1.75f / 2f * newHeight, cameraShiftDuration)
            .SetEase(cameraShiftEaseType);
    }
}