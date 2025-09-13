using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;

namespace PlayerStates
{
    [System.Serializable]
    public class GroundedSlideState : State<PlayerController>
    {
        [field: SerializeField] public float StartSlideSpeedThreshold { get; private set; } = 5f;
        [SerializeField] private float enterSpeedMultiplier = 1.5f;
        [SerializeField] private float friction = 5f;
        [SerializeField] private float controllerHeight = 1f;
        
        [Header("Camera Shift")]
        [SerializeField] private float enterCameraShiftDuration = 0.25f;
        [SerializeField] private Ease enterCameraShiftEaseType = Ease.Linear;
        [SerializeField] private float exitCameraShiftDuration = 0.25f;
        [SerializeField] private Ease exitCameraShiftEaseType = Ease.Linear;

        private float currentSlideSpeed;
        private float originalControllerHeight;

        private protected override void OnInit()
        {
            originalControllerHeight = context.Controller.height;
        }

        private protected override void OnEnter()
        {
            currentSlideSpeed = enterSpeedMultiplier * context.PlanarVelocity.magnitude;
            
            context.ChangeControllerHeight(controllerHeight, enterCameraShiftDuration, enterCameraShiftEaseType);
        }

        private protected override void OnExit()
        {
            context.ChangeControllerHeight(originalControllerHeight, exitCameraShiftDuration, exitCameraShiftEaseType);
        }

        private protected override void OnUpdate()
        {
            currentSlideSpeed = Mathf.Lerp(currentSlideSpeed, 0f, friction * Time.deltaTime);
            
            Vector3 moveDirection = Utils.GetCameraBasedMoveInput(CameraManager.Instance.CurrentCamera.transform, context.Input.MoveDirection);
            moveDirection = moveDirection == Vector3.zero ? CameraManager.Instance.CurrentCamera.transform.forward.WithY(0f) : moveDirection;
            Vector3 newVelocity = currentSlideSpeed * moveDirection;
            
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

            if (currentSlideSpeed <= context.GroundedSuperState.CrouchState.Speed)
                return context.GroundedSuperState.CrouchState;
            
            return null;
        }
    }
}