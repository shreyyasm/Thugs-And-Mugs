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
            _aManager.ChangeState(_aManager.aLocomotion);
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
