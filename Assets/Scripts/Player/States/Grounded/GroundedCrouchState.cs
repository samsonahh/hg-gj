using DG.Tweening;
using UnityEngine;

namespace PlayerStates
{
    [System.Serializable]
    public class GroundedCrouchState : State<PlayerController>
    {
        [field: SerializeField] public float Speed { get; private set; } = 1.5f;
        [SerializeField] private float decceleration = 5f;
        [SerializeField] private float controllerHeight = 1f;
        
        [Header("Camera Shift")]
        [SerializeField] private float enterCameraShiftDuration = 0.25f;
        [SerializeField] private Ease enterCameraShiftEaseType = Ease.Linear;
        [SerializeField] private float exitCameraShiftDuration = 0.25f;
        [SerializeField] private Ease exitCameraShiftEaseType = Ease.Linear;

        private float originalControllerHeight;
        
        private protected override void OnInit()
        {
            originalControllerHeight = context.Controller.height;
        }
        
        private protected override void OnEnter()
        {
            context.ChangeControllerHeight(controllerHeight, enterCameraShiftDuration, enterCameraShiftEaseType);
        }

        private protected override void OnExit()
        {
            context.ChangeControllerHeight(originalControllerHeight, exitCameraShiftDuration, exitCameraShiftEaseType);
        }

        private protected override void OnUpdate()
        {
            Vector3 moveDirection = Utils.GetCameraBasedMoveInput(CameraManager.Instance.CurrentCamera.transform, context.Input.MoveDirection);
            
            Vector3 targetVelocity = Speed * moveDirection;
            Vector3 newVelocity = targetVelocity;
            if(context.PlanarVelocity.magnitude > Speed)
                newVelocity = Vector3.Lerp(context.PlanarVelocity, targetVelocity, decceleration * Time.deltaTime);
            
            context.SetPlanarVelocity(newVelocity);
            context.ApplyPlanarVelocity();
        }

        private protected override void OnFixedUpdate()
        {

        }

        private protected override State<PlayerController> GetTransition()
        {
            if (!context.Input.InputActions.Player.Crouch.IsPressed())
                return context.GroundedSuperState.IdleState;
            
            return null;
        }
    }
}