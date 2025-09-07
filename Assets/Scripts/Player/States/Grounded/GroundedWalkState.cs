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

        }

        private protected override void OnFixedUpdate()
        {
            Vector3 moveDirection = Utils.GetCameraBasedMoveInput(CameraManager.Instance.CurrentCamera.transform, context.Input.MoveDirection);

            context.RigidBody.MovePosition(context.transform.position + speed * Time.fixedDeltaTime * moveDirection);
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (context.Input.MoveDirection == Vector2.zero)
                return context.GroundedSuperState.IdleState;

            return null;
        }
    }
}