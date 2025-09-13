using UnityEngine;

namespace PlayerStates
{
    [System.Serializable]
    public class GroundedIdleState : State<PlayerController>
    {
        [SerializeField] private float decceleration = 20f;
        [SerializeField] private float stopSpeedThreshold = 0.01f;

        private protected override void OnEnter()
        {

        }

        private protected override void OnExit()
        {

        }

        private protected override void OnUpdate()
        {
            Vector3 newVelocity = Vector3.Lerp(context.PlanarVelocity, Vector3.zero, decceleration * Time.deltaTime);
            if(newVelocity.magnitude <= stopSpeedThreshold)
                newVelocity = Vector3.zero;
            
            context.SetPlanarVelocity(newVelocity);
            context.ApplyPlanarVelocity();
        }

        private protected override void OnFixedUpdate()
        {

        }

        private protected override State<PlayerController> GetTransition()
        {
            if (context.Input.MoveDirection != Vector2.zero)
            {
                if (context.Input.InputActions.Player.Sprint.IsPressed())
                    return context.GroundedSuperState.SprintState;
                
                return context.GroundedSuperState.WalkState;
            }
            
            return null;
        }
    }
}