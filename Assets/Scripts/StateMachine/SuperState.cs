using UnityEngine;

[System.Serializable]
public abstract class SuperState<TContext> : State<TContext> where TContext : MonoBehaviour
{
    private protected StateMachine<TContext> subStateMachine;
    public abstract State<TContext> InitialSubState { get; }

    private protected override void OnInit()
    {
        subStateMachine = new StateMachine<TContext>(context);
        InitializeSubStates();
    }

    private protected abstract void InitializeSubStates();

    public override void Destroy()
    {
        subStateMachine.Destroy();
        base.Destroy();
    }

    public override void Enter()
    {
        base.Enter();
        subStateMachine.ChangeState(InitialSubState, true);
    }

    public override void Exit()
    {
        subStateMachine.ExitCurrentState();
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        State<TContext> transitionState = GetSubStateTransition();
        if (transitionState != null && !subStateMachine.HasTransitionedThisFrame)
        {
            subStateMachine.ChangeState(transitionState);
            return;
        }
        subStateMachine.Update();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        subStateMachine.FixedUpdate();
    }

    /// <summary>
    /// Polls for a sub-state transition. If a sub-state transition is returned, the SubStateMachine will handle the transition.
    /// </summary>
    private protected virtual State<TContext> GetSubStateTransition() => null;
}