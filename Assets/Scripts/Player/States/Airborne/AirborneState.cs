namespace PlayerStates
{
    [System.Serializable]
    public class AirborneState : SuperState<PlayerController>
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
    }
}