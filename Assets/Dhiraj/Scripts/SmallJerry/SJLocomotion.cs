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

            if (_sJManager.waypointBank.path.Count > 0)
            {
                MoveToNextWaypoint();
            }
        }

        public override void UpdateState()
        {
            base.UpdateState();
            
            if (!_sJManager.agent.pathPending && _sJManager.agent.remainingDistance <= _sJManager.agent.stoppingDistance)
            {
                if (!_sJManager.agent.hasPath || _sJManager.agent.velocity.sqrMagnitude < 0.1f)
                {
                    currentWaypointIndex++;

                    if (currentWaypointIndex >= _sJManager.waypointBank.path.Count)
                    {
                        _sJManager.ChangeState(_sJManager.Action);
                    }
                    else
                    {
                        MoveToNextWaypoint();
                    }
                }
            }

        }

        public override void ExitState()
        {
            base.ExitState();
            _sJManager.agent.ResetPath(); // Optional cleanup
        }


        private void MoveToNextWaypoint()
        {
            Transform target = _sJManager.waypointBank.path[currentWaypointIndex];
            _sJManager.agent.SetDestination(target.position);
        }

    }
}
