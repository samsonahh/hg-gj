using UnityEngine;

namespace PlayerStates
{
    [System.Serializable]
    public class AirborneMoveState : State<PlayerController>
    {
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
            Vector3 targetVelocity = context.PlanarVelocity + acceleration * Time.deltaTime * moveDirection;
            
            context.SetPlanarVelocity(targetVelocity);
            context.ApplyPlanarVelocity();
        }

        private protected override void OnFixedUpdate()
        {
            
        }

        private protected override State<PlayerController> GetTransition()
        {
            if(context.Input.MoveDirection == Vector2.zero)
                return context.AirborneSuperState.IdleState;
            
            return base.GetTransition();
        }
    }
}