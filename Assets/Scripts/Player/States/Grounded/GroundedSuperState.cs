using NaughtyAttributes;
using UnityEngine;

namespace PlayerStates
{
    [System.Serializable]
    public class GroundedSuperState : SuperState<PlayerController>
    {
        public override State<PlayerController> InitialSubState => IdleState;

        [field: SerializeField] public GroundedIdleState IdleState { get; private set; }
        [field: SerializeField] public GroundedWalkState WalkState { get; private set; }
        [field: SerializeField] public GroundedSprintState SprintState { get; private set; }
        [field: SerializeField] public GroundedSlideState SlideState { get; private set; }

        [field: Header("Config")]
        [field: SerializeField] public float GroundCheckDistance { get; private set; } = 1f;
        [field: SerializeField] public float GroundCheckRadius { get; private set; } = 1f;
        [SerializeField] private LayerMask groundLayerMask;
        [SerializeField] private float jumpHeight = 1.5f;
        [field: SerializeField, ReadOnly, AllowNesting] public bool IsGrounded { get; private set; }

        private protected override void InitializeSubStates()
        {
            IdleState.Init(SubStateMachine, context);
            WalkState.Init(SubStateMachine, context);
            SprintState.Init(SubStateMachine, context);
            SlideState.Init(SubStateMachine, context);
        }

        private protected override void OnEnter()
        {
            context.Input.Jump += Input_Jump;
        }

        private protected override void OnExit()
        {
            if (context.Input != null)
            {
                context.Input.Jump -= Input_Jump;
            }
        }

        private protected override void OnUpdate()
        {
        
        }

        private protected override void OnFixedUpdate()
        {
        
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (!IsGrounded)
                return context.AirborneSuperState;

            return null;
        }

        public void CheckGrounded()
        {
            IsGrounded = Physics.CheckSphere(context.transform.position + GroundCheckDistance * Vector3.down, GroundCheckRadius, groundLayerMask);
        }

        private void Input_Jump()
        {
            if (jumpHeight <= 0f)
                return;

            if (!IsGrounded)
                return;

            float jumpForce = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics.gravity.y)); // Equation to calculate jump force based on desired height

            stateMachine.ChangeState(context.AirborneSuperState);
        }
    }
}