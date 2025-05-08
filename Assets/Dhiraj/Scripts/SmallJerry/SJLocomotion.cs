using UnityEngine;

namespace Dhiraj
{
    public class SJLocomotion : SJBase
    {
        private int currentWaypointIndex = 0;
        public SJLocomotion(SJManager sJManager):base(sJManager)
        {
            _sJManager = sJManager;
        }


        public override void StartState()
        {
            base.StartState();
            currentWaypointIndex = 0;

            if (_sJManager.waypointBank.path.Count > 0 && !_sJManager.GoReturn)
            {
                MoveToNextWaypoint(currentWaypointIndex);
            }
            else
            {
                MoveToNextWaypoint(currentWaypointIndex - 1);
            }

            if (_sJManager.isWalkingWithBarrel) ChangeAnimationState(CurrentState.WithBarrel); else ChangeAnimationState(CurrentState.Normal);
        }

        public override void UpdateState()
        {
            base.UpdateState();

            bool isAgentStopped = !_sJManager.agent.pathPending &&
                      _sJManager.agent.remainingDistance <= _sJManager.agent.stoppingDistance &&
                      (!_sJManager.agent.hasPath || _sJManager.agent.velocity.sqrMagnitude < 0.1f);

            if (isAgentStopped)
            {
                // Determine direction of waypoint traversal
                int direction = _sJManager.GoReturn ? -1 : 1;
                currentWaypointIndex += direction;

                // Handle bounds and transitions
                if (!_sJManager.GoReturn && currentWaypointIndex >= _sJManager.waypointBank.path.Count)
                {
                    _sJManager.ChangeState(_sJManager.Action);
                }
                else if (_sJManager.GoReturn && currentWaypointIndex <= 0)
                {
                    Debug.Log("Process Over");
                }
                else
                {
                    MoveToNextWaypoint(currentWaypointIndex);
                }
            }


        }

        public override void ExitState()
        {
            base.ExitState();
            _sJManager.agent.ResetPath(); // Optional cleanup
        }

    }
}
