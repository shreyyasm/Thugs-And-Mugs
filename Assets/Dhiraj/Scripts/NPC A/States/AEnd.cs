using UnityEngine;

namespace Dhiraj
{

    public class AEnd : ABase
    {
        public AEnd(AManager aManager) : base(aManager)
        {
            _aManager = aManager;
        }

        public override void StartState()
        {
            base.StartState();
            Debug.Log("Stopped");
            _aManager.Death();
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
