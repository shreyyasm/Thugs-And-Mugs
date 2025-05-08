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
            
            LeanTween.delayedCall(0.5f, () => {
                _sJManager.ChangeState(_sJManager.Locomotion);
                _sJManager.GoReturn = true;
                //// Spwan Barrel at the current position in the some distance
                ChangeAnimationState(CurrentState.Normal);
                
            });
                       
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
