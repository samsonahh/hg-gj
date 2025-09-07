using UnityEngine;
using PlayerStates;

public class PlayerController : MonoBehaviour
{
    private StateMachine<PlayerController> stateMachine;
    [field: Header("States")]
    [field: SerializeField] public GroundedState GroundedState { get; private set; } = new();
    [field: SerializeField] public WallRunState WallRunState { get; private set; } = new();

    private void Awake()
    {
        InitializeStateMachine();
    }

    private void InitializeStateMachine()
    {
        stateMachine = new StateMachine<PlayerController>(this);

        GroundedState.Init(stateMachine, this);
        WallRunState.Init(stateMachine, this);

        stateMachine.ChangeState(GroundedState, true);
    }

    private void OnDestroy()
    {
        stateMachine.Destroy();
    }

    private void Update()
    {
        stateMachine.Update();
    }

    private void FixedUpdate()
    {
        stateMachine.FixedUpdate();
    }
}