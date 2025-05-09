using UnityEngine;
using static Dhiraj.SJBase;

namespace Dhiraj
{
    public abstract class ABase
    {
        public class AnimHash
        {
            public const string Idle = "Idle";
            public const string LookingAround = "Looking Around";
            public const string SitAndDrinking = "Sit and Drinking";
            public const string SittingWaiting = "Sitting Waiting";
            public const string Walking = "Walking";
        }
        public ABase(AManager aManager)
        {
            _aManager = aManager;
        }
        public enum CurrentState
        {
            Idle = 0,
            Walking = 1,
            LookingAround = 2,
            Waiting = 3,            
            Drinking = 4            
        }

        protected AManager _aManager;        
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
                    _aManager.anim.Play(AnimHash.Walking);                 ;
                    break;
                case CurrentState.Waiting:
                    _aManager.anim.Play(AnimHash.SittingWaiting);                    
                    break;
                case CurrentState.LookingAround:
                    _aManager.anim.Play(AnimHash.LookingAround);
                    break;
                case CurrentState.Drinking:
                    _aManager.anim.Play(AnimHash.SitAndDrinking);
                    break;

            }
        }

        public void MoveToNextWaypoint(int currentWaypointIndex)
        {
            Transform target = _aManager.waypointBank.path[currentWaypointIndex];
            _aManager.agent.SetDestination(target.position);
            ChangeAnimationState(CurrentState.Walking);
        }

        public void MoveToWaypoint(Vector3 pos)
        {
            _aManager.agent.SetDestination(pos);
            ChangeAnimationState(CurrentState.Walking);
        }
    }
}

