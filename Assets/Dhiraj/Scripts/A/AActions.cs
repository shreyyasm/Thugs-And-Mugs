using UnityEngine;

namespace Dhiraj
{
    public class AActions : ABase
    {
        public AActions(AManager aManager) : base(aManager)
        {
            _aManager = aManager;
        }

        public override void StartState()
        {
            base.StartState();
            Debug.Log("Sitting");
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
