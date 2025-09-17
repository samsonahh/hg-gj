using System;
using System.Collections;
using UnityEngine;

namespace PlayerStates
{
    [System.Serializable]
    public class AirborneSuperState : SuperState<PlayerController>
    {
        public override State<PlayerController> InitialSubState => IdleState;

        [field: SerializeField] public AirborneIdleState IdleState { get; private set; } = new();
        [field: SerializeField] public AirborneMoveState MoveState { get; private set; } = new();
        
        [Header("Config")] 
        [SerializeField] private float regroundTimeThreshold = 0.1f;
        public float Timer { get; private set; }

        (float timer, Action jumpFunction)? coyoteTimeData;

        private protected override void InitializeSubStates()
        {
            IdleState.Init(SubStateMachine, context);
            MoveState.Init(SubStateMachine, context);
        }

        private protected override void OnEnter()
        {
            Timer = 0f;
        }

        private protected override void OnExit()
        {
            if(coyoteTimeData != null && context.Input !=null)
                context.Input.Jump -= coyoteTimeData.Value.jumpFunction;
        }

        private protected override void OnUpdate()
        {
            Timer += Time.deltaTime;

            UpdateCoyoteTime();
            
            context.ApplyGravity();
        }

        private protected override void OnFixedUpdate()
        {

        }

        private protected override State<PlayerController> GetTransition()
        {
            if (context.IsGrounded && Timer > regroundTimeThreshold)
                return context.GroundedSuperState;
            
            if(context.WallRunState.IsWallRunning())
                return context.WallRunState;

            return null;
        }

        public void ActivateCoyoteTime(float duration, Action jumpCallback)
        {
            coyoteTimeData = (duration, jumpCallback);
            context.Input.Jump += jumpCallback;
        }

        private void UpdateCoyoteTime()
        {
            if (coyoteTimeData == null)
                return;
            
            coyoteTimeData = (coyoteTimeData.Value.timer - Time.deltaTime, coyoteTimeData.Value.jumpFunction);
            if(coyoteTimeData.Value.timer <= 0f)
            {
                if (context.Input != null)
                    context.Input.Jump -= coyoteTimeData.Value.jumpFunction;
                coyoteTimeData = null;
            }
        }
    }
}