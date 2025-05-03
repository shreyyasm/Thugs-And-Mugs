using UnityEngine;

namespace Dhiraj
{
    public class SJIdle : SJBase
    {
        public SJIdle(SJManager sJManager) : base(sJManager)
        {
            _sJManager = sJManager;
        }

        public override void StartState()
        {
            base.StartState();
            _sJManager.ChangeState(_sJManager.Locomotion);
        }

        public override void UpdateState()
        {
            base.UpdateState();
        }

        public override void ExitState()
        {
            base.ExitState();
        }
    }
}
