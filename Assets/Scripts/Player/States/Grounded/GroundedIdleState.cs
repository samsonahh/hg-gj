using UnityEngine;

namespace PlayerStates
{
    [System.Serializable]
    public class GroundedIdleState : State<PlayerController>
    {
        [SerializeField] private float friction = 1f;
        [SerializeField] private float stopSpeedThreshold = 0.05f;
        
        private protected override void OnEnter()
        {

        }

        private protected override void OnExit()
        {

        }

        private protected override void OnUpdate()
        {
            Vector3 planarVelocity = context.Velocity.WithY(0f);
            Vector3 newVelocity = Vector3.Lerp(planarVelocity, Vector3.zero, friction * Time.deltaTime);
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
                return context.GroundedSuperState.WalkState;

            return null;
        }
    }
}