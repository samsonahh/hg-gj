using UnityEngine;

namespace PlayerStates
{
    [System.Serializable]
    public class GroundedState : SuperState<PlayerController>
    {
        public override State<PlayerController> InitialSubState => IdleState;

        [field: Header("Sub States")]
        [field: SerializeField] public GroundedIdleState IdleState { get; private set; }
        [field: SerializeField] public GroundedWalkState WalkState { get; private set; }
        [field: SerializeField] public GroundedSprintState SprintState { get; private set; }
        [field: SerializeField] public GroundedSlideState SlideState { get; private set; }

        private protected override void InitializeSubStates()
        {
            IdleState.Init(SubStateMachine, context);
            WalkState.Init(SubStateMachine, context);
            SprintState.Init(SubStateMachine, context);
            SlideState.Init(SubStateMachine, context);
        }

        private protected override void OnEnter()
        {
        
        }

        private protected override void OnExit()
        {
        
        }

        private protected override void OnUpdate()
        {
        
        }

        private protected override void OnFixedUpdate()
        {
        
        }
    }
}