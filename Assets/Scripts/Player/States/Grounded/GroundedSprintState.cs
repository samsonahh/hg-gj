using UnityEngine;

namespace PlayerStates
{
    [System.Serializable]
    public class GroundedSprintState : State<PlayerController>
    {
        [SerializeField] private float targetSpeed = 6f;
        [SerializeField] private float acceleration = 5f;
        
        private protected override void OnEnter()
        {

        }

        private protected override void OnExit()
        {

        }

        private protected override void OnUpdate()
        {
            Vector3 moveDirection = Utils.GetCameraBasedMoveInput(CameraManager.Instance.CurrentCamera.transform, context.Input.MoveDirection);
            
            Vector3 targetVelocity = targetSpeed * moveDirection;
            Vector3 newVelocity = Vector3.Lerp(context.PlanarVelocity, targetVelocity, acceleration * Time.deltaTime);
            
            context.SetPlanarVelocity(newVelocity);
            context.ApplyPlanarVelocity();
        }

        private protected override void OnFixedUpdate()
        {

        }

        private protected override State<PlayerController> GetTransition()
        {
            if (!context.Input.InputActions.Player.Sprint.IsPressed())
                return context.GroundedSuperState.IdleState;
            
            return null;
        }
    }
}