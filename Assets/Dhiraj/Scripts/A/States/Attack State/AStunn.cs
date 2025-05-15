using UnityEngine;

namespace Dhiraj
{

    public class AStunn : ABase
    {
        public AStunn(AManager aManager) : base(aManager)
        {
            _aManager = aManager;
        }

        public override void StartState()
        {
            base.StartState();
            ChangeAnimationState(CurrentState.Stun);
        }

        public override void UpdateState()
        {
            //base.UpdateState();
        }

        public override void EndState()
        {
            base.EndState();
        }
    }
}
