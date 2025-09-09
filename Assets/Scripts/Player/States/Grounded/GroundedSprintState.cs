using UnityEngine;

namespace PlayerStates
{
    [System.Serializable]
    public class GroundedSprintState : State<PlayerController>
    {
        [Header("Movement")]
        [SerializeField] private float targetSpeed = 6f;
        [SerializeField] private float acceleration = 5f;

        [Header("VFX")]
        [SerializeField] private ParticleSystem sprintParticleSystem;

        private protected override void OnEnter()
        {
            // Play sprint particles.
            if (sprintParticleSystem != null && !sprintParticleSystem.isPlaying)
                sprintParticleSystem.Play();
        }

        private protected override void OnExit()
        {
            // Stop sprint particles when exiting sprint.
            if (sprintParticleSystem != null && sprintParticleSystem.isPlaying)
                sprintParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private protected override void OnUpdate()
        {
            Vector3 moveDirection = Utils.GetCameraBasedMoveInput(
                CameraManager.Instance.CurrentCamera.transform,
                context.Input.MoveDirection);

            Vector3 targetVelocity = targetSpeed * moveDirection;
            Vector3 newVelocity = Vector3.Lerp(
                context.PlanarVelocity,
                targetVelocity,
                acceleration * Time.deltaTime);

            context.SetPlanarVelocity(newVelocity);
            context.ApplyPlanarVelocity();
        }

        private protected override void OnFixedUpdate()
        {
            // Intentionally left blank.
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (!context.Input.InputActions.Player.Sprint.IsPressed())
                return context.GroundedSuperState.IdleState;

            return null;
        }
    }
}