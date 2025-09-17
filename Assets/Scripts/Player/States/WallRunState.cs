using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace PlayerStates
{
    [System.Serializable]
    public class WallRunState : State<PlayerController>
    {
        [SerializeField] private float minimumSpeed = 5f;
        [SerializeField] private float climbSpeed = 10f;
        [SerializeField] private float jumpOffHeight = 1.5f;
        
        [SerializeField] private float wallCheckDistance = 0.5f;
        [SerializeField] private float wallCheckYOffset = 0f;
        [SerializeField] private LayerMask wallLayerMask;
        
        private Ray rightRay => new Ray(context.transform.position + wallCheckYOffset * Vector3.up, context.transform.right);
        private Ray leftRay => new Ray(context.transform.position + wallCheckYOffset * Vector3.up, -context.transform.right);
        
        private bool isTouchingRightWall;
        private bool isTouchingLeftWall;
        private RaycastHit rightHit;
        private RaycastHit leftHit;
        private Vector3 wallNormal => isTouchingRightWall ? rightHit.normal : leftHit.normal;

        private Vector3 enterVelocity;

        private protected override void OnInit()
        {
            context.OnDrawGizmosActions += OnDrawGizmos;
        }

        private protected override void OnEnter()
        {
            context.SetVelocity(context.Velocity.WithY(0f));
            enterVelocity = context.Velocity;
            
            context.Input.Jump += Input_Jump;
        }

        private protected override void OnExit()
        {
            if (context.Input != null)
                context.Input.Jump -= Input_Jump;
        }

        private protected override void OnUpdate()
        {
            /*Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up);
            if((context.transform.forward - wallForward).magnitude > (context.transform.forward + wallForward).magnitude) // Wall forward is based on player forward
                wallForward = -wallForward;*/
            
            Vector3 wallUp = Vector3.ProjectOnPlane(Vector3.up, wallNormal).normalized;
            
            context.SetVelocity(enterVelocity + climbSpeed * context.Input.MoveDirection.y * wallUp);
            
            context.ApplyPlanarVelocity();
            context.ApplyGravity(false);
        }

        private protected override void OnFixedUpdate()
        {

        }

        private protected override State<PlayerController> GetTransition()
        {
            if (!IsWallRunning())
                return context.AirborneSuperState;
            
            return null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(rightRay.origin, rightRay.origin + wallCheckDistance * rightRay.direction);
            Gizmos.DrawLine(leftRay.origin, leftRay.origin + wallCheckDistance * leftRay.direction);
        }
        
        private void Input_Jump()
        {
            float jumpForce = Utils.GetJumpForce(jumpOffHeight);
            context.SetVelocity(jumpForce * (wallNormal + Vector3.up));
            
            stateMachine.ChangeState(context.AirborneSuperState);
        }

        public bool IsWallRunning()
        {
            isTouchingRightWall = Physics.Raycast(rightRay.origin, rightRay.direction, out rightHit, wallCheckDistance, wallLayerMask);
            isTouchingLeftWall = Physics.Raycast(leftRay.origin, leftRay.direction, out leftHit, wallCheckDistance, wallLayerMask);
            
            if (!context.Input.InputActions.Player.Sprint.IsPressed())
                return false;
            
            if (context.PlanarVelocity.magnitude < minimumSpeed)
                return false;

            return isTouchingRightWall ||isTouchingLeftWall;
        }
    }
}