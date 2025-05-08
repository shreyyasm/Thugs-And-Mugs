using UnityEngine;

namespace Dhiraj
{
    public class ALocomotion : ABase
    {
        private int currentWaypointIndex = 0;
        public ALocomotion(AManager aManager) : base(aManager)
        {
            _aManager = aManager;
        }

        public override void StartState()
        {
            base.StartState();
            if (!_aManager.isLookAround)
            {
                currentWaypointIndex = 0;

                if (_aManager.waypointBank.path.Count > 0 && !_aManager.GoReturn)
                {
                    MoveToNextWaypoint(currentWaypointIndex);
                }
                else
                {
                    MoveToNextWaypoint(currentWaypointIndex - 1);
                }
            }
                            
        }

        public override void UpdateState()
        {
            base.UpdateState();
            if (!_aManager.isLookAround) 
                WaypointFollow();
            else if (_aManager.isLookAround)
            {
                bool isAgentStopped = !_aManager.agent.pathPending &&
                      _aManager.agent.remainingDistance <= _aManager.agent.stoppingDistance &&
                      (!_aManager.agent.hasPath || _aManager.agent.velocity.sqrMagnitude < 0.1f);
                if (isAgentStopped)
                {
                    _aManager.ChangeState(_aManager.aActions);
                }
            }
        }



        public void WaypointFollow()
        {
            bool isAgentStopped = !_aManager.agent.pathPending &&
                       _aManager.agent.remainingDistance <= _aManager.agent.stoppingDistance &&
                       (!_aManager.agent.hasPath || _aManager.agent.velocity.sqrMagnitude < 0.1f);

            if (isAgentStopped)
            {
                // Determine direction of waypoint traversal
                int direction = _aManager.GoReturn ? -1 : 1;
                currentWaypointIndex += direction;

                // Handle bounds and transitions
                if (!_aManager.GoReturn && currentWaypointIndex >= _aManager.waypointBank.path.Count)
                {
                    _aManager.ChangeState(_aManager.aLookAround);
                }
                else if (_aManager.GoReturn && currentWaypointIndex <= 0)
                {
                    Debug.Log("Process Over");
                }
                else
                {
                    MoveToNextWaypoint(currentWaypointIndex);
                }
            }
        }

        public override void EndState()
        {
            base.EndState();
            _aManager.agent.ResetPath(); // Optional cleanup
        }
    }
}
