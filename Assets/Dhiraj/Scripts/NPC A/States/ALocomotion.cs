using System.Net;
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


            if (!_aManager.GoReturn && !_aManager.isLookAround)
            {
                currentWaypointIndex = 0;
                if (_aManager.waypointBank.path.Count > 0) MoveToNextWaypoint(currentWaypointIndex);
            }
            else if(_aManager.GoReturn && _aManager.isLookAround)
            {
                currentWaypointIndex = _aManager.waypointBank.path.Count;
                if (_aManager.waypointBank.path.Count > 0) MoveToNextWaypoint(currentWaypointIndex - 1);

            }

        }

        public override void UpdateState()
        {
            base.UpdateState();
            if (!_aManager.isLookAround &&  !_aManager.GoReturn)
            {
                WaypointFollow();
                return;
            }
            else if (_aManager.isLookAround && !_aManager.GoReturn)
            {
                bool isAgentStopped = !_aManager.agent.pathPending &&
                      _aManager.agent.remainingDistance <= _aManager.agent.stoppingDistance &&
                      (!_aManager.agent.hasPath || _aManager.agent.velocity.sqrMagnitude < 0.1f);
                if (isAgentStopped)
                {
                    _aManager.ChangeState(_aManager.aActions);
                }
            }
            else if (_aManager.GoReturn)
            {
                WaypointFollow();
            }
        }



        public void WaypointFollow()
        {
            bool isAgentStopped = !_aManager.agent.pathPending &&
                       _aManager.agent.remainingDistance <= _aManager.agent.stoppingDistance &&
                       (!_aManager.agent.hasPath || _aManager.agent.velocity.sqrMagnitude < 0.1f);

            if (isAgentStopped)
            {
                int direction = _aManager.GoReturn ? -1 : 1;
                currentWaypointIndex += direction;

                if (!_aManager.GoReturn)
                {
                    if (currentWaypointIndex >= _aManager.waypointBank.path.Count)
                    {
                        _aManager.ChangeState(_aManager.aLookAround);
                        return;
                    }
                }
                else
                {
                    if (currentWaypointIndex < 0)
                    {
                        Debug.Log("Process Over");
                        _aManager.GoReturn = false;
                        _aManager.isLookAround = false;
                        _aManager.ChangeState(_aManager.aEnd);
                        return;
                    }
                }

                MoveToNextWaypoint(currentWaypointIndex);
            }
        }


        public override void EndState()
        {
            base.EndState();
            _aManager.agent.ResetPath(); // Optional cleanup
        }
    }
}
