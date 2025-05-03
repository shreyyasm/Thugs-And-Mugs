using UnityEngine;

namespace Dhiraj
{
    public class SJAction : SJBase
    {
        public SJAction(SJManager sJManager) : base(sJManager)
        {
            _sJManager = sJManager;
        }

        public override void StartState()
        {
            base.StartState();
            Debug.Log("Small Jerry Reached");
        }
        public override void UpdateState() { 
            base.UpdateState();
        }

        public override void ExitState()
        {
            base.ExitState();
        }
    }
}
