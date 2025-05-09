using UnityEngine;

namespace Dhiraj
{
    public class ALookAround : ABase
    {
        SeatManager _seatManager;
        public ALookAround(AManager aManager) : base(aManager)
        {
            _aManager = aManager;
        }

        public override void StartState()
        {
            base.StartState();
            _seatManager = GameManager.Instance._SeatManager;
            ChangeAnimationState(CurrentState.LookingAround);
            LeanTween.delayedCall(2, () => { 
                _aManager.seat = _seatManager.GetEmptySeat();
                MoveToWaypoint(_aManager.seat.sitingPosition.position);
                _aManager.isLookAround = true;
                _aManager.ChangeState(_aManager.aLocomotion);
            });
            Debug.Log("Look Around");
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
