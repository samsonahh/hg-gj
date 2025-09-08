using UnityEngine;

namespace PlayerStates
{
    [System.Serializable]
    public class GroundedWalkState : State<PlayerController>
    {
        [SerializeField] private float speed = 3f;
        [SerializeField] private float decceleration = 5f;

        private protected override void OnEnter()
        {

        }

        private protected override void OnExit()
        {

        }

        private protected override void OnUpdate()
        {
            Vector3 moveDirection = Utils.GetCameraBasedMoveInput(CameraManager.Instance.CurrentCamera.transform, context.Input.MoveDirection);
            
            Vector3 targetVelocity = speed * moveDirection;
            Vector3 newVelocity = targetVelocity;
            if(context.PlanarVelocity.magnitude > speed)
                newVelocity = Vector3.Lerp(context.PlanarVelocity, targetVelocity, decceleration * Time.deltaTime);
            
            context.SetPlanarVelocity(newVelocity);
            context.ApplyPlanarVelocity();
        }

        private protected override void OnFixedUpdate()
        {

        }

        private protected override State<PlayerController> GetTransition()
        {
            if (context.Input.MoveDirection == Vector2.zero)
                return context.GroundedSuperState.IdleState;

            return null;
        }
    }
}