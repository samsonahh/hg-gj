using UnityEngine;

namespace PlayerStates
{
    [System.Serializable]
    public class AirborneIdleState : State<PlayerController>
    {
        private protected override void OnEnter()
        {
            
        }

        private protected override void OnExit()
        {
            
        }

        private protected override void OnUpdate()
        {
            context.ApplyPlanarVelocity();
        }

        private protected override void OnFixedUpdate()
        {
            
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (context.Input.MoveDirection != Vector2.zero)
                return context.AirborneSuperState.MoveState;
            
            return base.GetTransition();
        }
    }
}