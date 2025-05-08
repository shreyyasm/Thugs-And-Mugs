using UnityEngine;
using static Dhiraj.SJBase;

namespace Dhiraj
{
    public abstract class ABase
    {
        public class AnimHash
        {
            public const string NormalWalk = "Normal Walk";
            public const string WalkWithBarrel = "Walk With Barrel";
        }
        public ABase(AManager aManager)
        {
            _aManager = aManager;
        }
        public enum CurrentState
        {
            Walking,
            Sitting,
            Drinking,
            Leaving
        }
        
        public AManager _aManager;

        public virtual void StartState()
        {

        }
        public virtual void UpdateState()
        {

        }

        public virtual void EndState()
        {

        }


        public void ChangeAnimationState(CurrentState currentState)
        {
            switch (currentState)
            {
                case CurrentState.Walking:
                    _aManager.anim.Play(AnimHash.NormalWalk);                   ;
                    break;
                case CurrentState.Sitting:
                    _aManager.anim.Play(AnimHash.WalkWithBarrel);                    
                    break;

            }
        }

        public void MoveToNextWaypoint(int currentWaypointIndex)
        {
            Transform target = _aManager.waypointBank.path[currentWaypointIndex];
            _aManager.agent.SetDestination(target.position);
        }

        public void MoveToWaypoint(Vector3 pos)
        {
            _aManager.agent.SetDestination(pos);
        }
    }
}

