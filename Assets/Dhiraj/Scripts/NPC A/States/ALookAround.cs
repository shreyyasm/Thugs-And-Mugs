using UnityEngine;

namespace Dhiraj
{
    public class ALookAround : ABase
    {
        private SeatManager _seatManager;
        private const float lookDelay = 2f;

        public ALookAround(AManager aManager) : base(aManager)
        {
            _aManager = aManager;
        }

        public override void StartState()
        {
            base.StartState();

            _seatManager = GameManager.Instance._SeatManager;
            _aManager.seat = _seatManager.GetEmptySeat();
            ChangeAnimationState(CurrentState.LookingAround);

            if (_aManager.seat)
            {
                _aManager.seat.isOccupied = true; // 🔒 Immediately mark it reserved!
                _aManager.seat.occupiedBy = _aManager;
                TrySitAfterDelay();
                Debug.Log("Look Around (found seat)");
            }
            else if (!_aManager.seat && !_seatManager.GetEmptySeat())
            {
                Debug.Log("Look Around (no seat found initially)");
                _aManager.enemyTargets.Add(_seatManager.GetOccupiedSeat(_aManager));
            }
        }

        public override void UpdateState()
        {
            base.UpdateState();

            if (!_aManager.seat)
            {
                _aManager.seat = _seatManager.ReserveEmptySeat(_aManager);

                if (_aManager.seat)
                {
                    ChangeAnimationState(CurrentState.LookingAround);
                    _aManager.seat.isOccupied = true; // 🔒 Immediately mark it reserved!
                    _aManager.seat.occupiedBy = _aManager;
                    TrySitAfterDelay();
                    Debug.Log("Seat found during update");
                }
                else if (!_aManager.seat)
                {
                    Debug.Log("No seat is empty");
                    _aManager.seat = _seatManager.GetEmptySeat();
                }
            }
            else 
            {
                Debug.Log("No seat is empty");               
            }
        }

        public override void EndState()
        {
            base.EndState();
        }

        private void TrySitAfterDelay()
        {
            LeanTween.delayedCall(lookDelay, () =>
            {
                if (_aManager.seat == null) return;

                //_aManager.npcSpawnManager.canSpawn = true;
                MoveToWaypoint(_aManager.seat.sitingPosition.position);

                _aManager.isLookAround = true;

                _aManager.ChangeState(_aManager.aLocomotion);
            });
        }
    }
}
