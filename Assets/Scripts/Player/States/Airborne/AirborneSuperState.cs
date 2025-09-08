using System;
using System.Collections;
using UnityEngine;

namespace PlayerStates
{
    [System.Serializable]
    public class AirborneSuperState : SuperState<PlayerController>
    {
        public override State<PlayerController> InitialSubState => null;

        [Header("Config")] 
        [SerializeField] private float regroundTimeThreshold = 0.1f;
        public float Timer { get; private set; }

        (float timer, Action jumpFunction)? coyoteTimeData;

        private protected override void InitializeSubStates()
        {

        }

        private protected override void OnEnter()
        {
            Timer = 0f;
        }

        private protected override void OnExit()
        {
            if(coyoteTimeData != null)
            {
                if(context.Input != null)
                    context.Input.Jump -= coyoteTimeData.Value.jumpFunction;
            }
        }

        private protected override void OnUpdate()
        {
            Timer += Time.deltaTime;
            
            if(coyoteTimeData != null)
            {
                coyoteTimeData = (coyoteTimeData.Value.timer - Time.deltaTime, coyoteTimeData.Value.jumpFunction);
                if(coyoteTimeData.Value.timer <= 0f)
                {
                    if (context.Input != null)
                        context.Input.Jump -= coyoteTimeData.Value.jumpFunction;
                    coyoteTimeData = null;
                }
            }

            context.ApplyPlanarVelocity();
            context.ApplyGravity();
        }

        private protected override void OnFixedUpdate()
        {

        }

        private protected override State<PlayerController> GetTransition()
        {
            if (context.IsGrounded && Timer > regroundTimeThreshold)
                return context.GroundedSuperState;

            return null;
        }

        public void ActivateCoyoteTime(float duration, Action jumpCallback)
        {
            coyoteTimeData = (duration, jumpCallback);
            context.Input.Jump += jumpCallback;
        }
    }
}