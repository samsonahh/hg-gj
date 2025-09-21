using System;
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

    [field: Header("Config")]
    [field: SerializeField] public LayerMask GroundLayerMask { get; private set; }
    [field: SerializeField] public float StrictSpeedCap { get; private set; }  = 100f;

    [SerializeField] private float wallFriction = 0.8f;

    public StateMachine<PlayerController> StateMachine { get; private set; }
    [field: Header("States")]
    [field: SerializeField] public GroundedSuperState GroundedSuperState { get; private set; } = new();
    [field: SerializeField] public AirborneSuperState AirborneSuperState { get; private set; } = new();
    [field: SerializeField] public WallRunState WallRunState { get; private set; } = new();

    [ShowNativeProperty] public bool IsGrounded => GroundedSuperState.IsGrounded; // for easier access
    private Vector3 velocity;
    [ShowNativeProperty] public Vector3 Velocity => velocity;
    public Vector3 PlanarVelocity => velocity.WithY(0f);
    [ShowNativeProperty] private float currentSpeed => PlanarVelocity.magnitude;

    public Action OnDrawGizmosActions = delegate { };

    private void Awake()
    {
        Input = InputManager.Instance;

        InitializeStateMachine();
    }

    private void InitializeStateMachine()
    {
        StateMachine = new StateMachine<PlayerController>(this);

        GroundedSuperState.Init(StateMachine, this);
        AirborneSuperState.Init(StateMachine, this);
        WallRunState.Init(StateMachine, this);

        StateMachine.ChangeState(GroundedSuperState, true);
    }

    private void OnDestroy()
    {
        StateMachine.Destroy();

        OnDrawGizmosActions = null;
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
        
        OnDrawGizmosActions?.Invoke();
    }

    private void Update()
    {
        GroundedSuperState.CheckGrounded();

        StateMachine.Update();
        
        // Strict cap
        SetPlanarVelocity(Vector3.ClampMagnitude(PlanarVelocity, StrictSpeedCap));
        
        transform.rotation = Quaternion.Euler(0f, CameraManager.Instance.CurrentCamera.transform.eulerAngles.y, 0f); ;
    }

    private void FixedUpdate()
    {
        StateMachine.FixedUpdate();
    }
    
    public void SetVelocity(Vector3 newVelocity) => velocity = newVelocity;
    
    public void SetPlanarVelocity(Vector3 newVelocity) => velocity = new Vector3(newVelocity.x, velocity.y, newVelocity.z);

    public void ApplyPlanarVelocity()
    {
        Vector3 displacement = velocity.WithY(0) * Time.deltaTime;
        
        // Prevents velocity buildup while ramming into walls
        if (displacement.sqrMagnitude > 0f)
        {
            if (Physics.CapsuleCast(
                    transform.position + Vector3.up * Controller.radius,  // bottom of CC
                    transform.position + Vector3.up * (Controller.height - Controller.radius),     // top of CC
                    Controller.radius,
                    displacement.normalized,
                    out RaycastHit hit,
                    displacement.magnitude + 0.01f,
                    GroundLayerMask))
            {
                Vector3 wallNormal = hit.normal;
                
                float intoWall = Vector3.Dot(velocity, wallNormal);
                if (intoWall < 0f)
                {
                    velocity -= wallNormal * intoWall;
                    
                    if (Vector3.Dot(velocity, wallNormal) < 0.01f)
                        velocity = Vector3.ProjectOnPlane(velocity, wallNormal) * wallFriction;
                    
                    displacement = velocity.WithY(0) * Time.deltaTime;
                }
            }
        }

        Controller.Move(displacement);
    }

    public void ApplyGravity(bool decreaseVelocity = true)
    {
        if(decreaseVelocity)
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