using NaughtyAttributes;
using System;
using UnityEngine;

namespace PlayerStates
{
    [System.Serializable]
    public class GroundedSuperState : SuperState<PlayerController>
    {
        public override State<PlayerController> InitialSubState => IdleState;

        [field: SerializeField] public GroundedIdleState IdleState { get; private set; } = new();
        [field: SerializeField] public GroundedWalkState WalkState { get; private set; } = new();
        [field: SerializeField] public GroundedSprintState SprintState { get; private set; } = new();
        [field: SerializeField] public GroundedCrouchState CrouchState { get; private set; } = new();
        [field: SerializeField] public GroundedSlideState SlideState { get; private set; } = new();

        [field: Header("Config")]
        [field: SerializeField] public float GroundCheckDistance { get; private set; } = 1f;
        [field: SerializeField] public float GroundCheckRadius { get; private set; } = 1f;
        [SerializeField] private LayerMask groundLayerMask;
        [SerializeField] private float groundedYVelocity = -5f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float coyoteTime = 0.5f;
        private bool hasJumped;
        public bool IsGrounded { get; private set; }

        private protected override void InitializeSubStates()
        {
            IdleState.Init(SubStateMachine, context);
            WalkState.Init(SubStateMachine, context);
            SprintState.Init(SubStateMachine, context);
            CrouchState.Init(SubStateMachine, context);
            SlideState.Init(SubStateMachine, context);
        }

        private protected override void OnEnter()
        {
            context.SetVelocity(new Vector3(context.PlanarVelocity.x, groundedYVelocity, context.PlanarVelocity.z));

            hasJumped = false;
            
            context.Input.Jump += Input_Jump;
            context.Input.Crouch += Input_Crouch;
            
            context.WallRunState.ResetPreviousWall();
            
            if (context.Input.InputActions.Player.Crouch.IsPressed())
            {
                SubStateMachine.ChangeState(SlideState);
                return;
            }
        }

        private protected override void OnExit()
        {
            if (context.Input != null)
            {
                context.Input.Jump -= Input_Jump;
                context.Input.Crouch -= Input_Crouch;
            }
        }

        private protected override void OnUpdate()
        {
            context.ApplyGravity(false);
        }

        private protected override void OnFixedUpdate()
        {
        
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (!IsGrounded)
            {
                if (!hasJumped)
                    context.AirborneSuperState.ActivateCoyoteTime(coyoteTime, Input_Jump);
                return context.AirborneSuperState;
            }
            
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

            if (hasJumped)
                return;

            float jumpForce = Utils.GetJumpForce(jumpHeight);
            context.SetVelocity(context.Velocity.WithY(jumpForce));
            hasJumped = true;
            
            stateMachine.ChangeState(context.AirborneSuperState);
        }
        
        private void Input_Crouch(bool isCrouching)
        {
            if (!isCrouching)
                return;

            if (context.PlanarVelocity.magnitude > SlideState.StartSlideSpeedThreshold)
            {
                SubStateMachine.ChangeState(SlideState);
                return;
            }
            
            SubStateMachine.ChangeState(CrouchState);
        }
    }
}