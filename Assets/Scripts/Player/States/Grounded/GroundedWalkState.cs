using UnityEngine;

namespace PlayerStates
{
    [System.Serializable]
    public class GroundedWalkState : State<PlayerController>
    {
        [SerializeField] private float speed = 3f;

        private protected override void OnEnter()
        {

        }

        private protected override void OnExit()
        {

        }

        private protected override void OnUpdate()
        {
            Vector3 moveDirection = Utils.GetCameraBasedMoveInput(CameraManager.Instance.CurrentCamera.transform, context.Input.MoveDirection);

            context.Controller.Move(speed * Time.deltaTime * moveDirection);
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