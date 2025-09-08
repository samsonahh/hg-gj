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

        private float coyoteTimer;

        private protected override void InitializeSubStates()
        {

        }

        private protected override void OnEnter()
        {
            Timer = 0f;
        }

        private protected override void OnExit()
        {
            if (context.Input != null)
            {
                context.Input.Jump -= context.GroundedSuperState.Input_Jump;
            }
        }

        private protected override void OnUpdate()
        {
            Timer += Time.deltaTime;
            
            HandleCoyoteTimer();
            
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

        public void ActivateCoyoteTime(float duration)
        {
            coyoteTimer = duration;
            context.Input.Jump += context.GroundedSuperState.Input_Jump;
        }

        private void HandleCoyoteTimer()
        {
            if (coyoteTimer > 0)
                coyoteTimer -= Time.deltaTime;
            else
            {
                coyoteTimer = 0f;
                if(context.Input != null)
                    context.Input.Jump -= context.GroundedSuperState.Input_Jump;
            }
        }
    }
}