using UnityEngine;

namespace Dhiraj
{


    public class AIdle : ABase
    {
        public AIdle(AManager aManager) : base(aManager)
        {
            _aManager = aManager;
        }

        public override void StartState()
        {
            base.StartState();
            if(_aManager.waypointBank)_aManager.ChangeState(_aManager.aLocomotion);
            ChangeAnimationState(CurrentState.Idle);
            _aManager.agent.enabled = true;
        }

        public override void UpdateState()
        {
            base.UpdateState();
        }

        public override void EndState()
        {
            base.EndState();
        }
    }
}
