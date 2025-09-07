namespace PlayerStates
{
    [System.Serializable]
    public class AirborneSuperState : SuperState<PlayerController>
    {
        public override State<PlayerController> InitialSubState => null;

        [field: Header("Sub States")]

        private protected override void InitializeSubStates()
        {

        }

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

        }

        private protected override State<PlayerController> GetTransition()
        {
            if (context.IsGrounded)
                return context.GroundedSuperState;

            return null;
        }
    }
}