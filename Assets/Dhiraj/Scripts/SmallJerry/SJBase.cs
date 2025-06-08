using UnityEngine;

namespace Dhiraj
{

    public abstract class SJBase
    {
        public class AnimHash {
            public const string Idle = "Idle";
            public const string NormalWalk = "Normal Walk";
            public const string WalkWithBarrel = "Walk With Barrel";
        }
        public enum CurrentState
        {
            Idle,
            Normal,
            WithBarrel
        }

        public SJManager _sJManager;
        public CurrentState animationState;
        public SJBase(SJManager sJanager)
        {
            _sJManager = sJanager;
        }

        public virtual void StartState()
        {

        }
        public virtual void UpdateState()
        {

        }
        public virtual void ExitState()
        {

        }

        public void ChangeAnimationState(CurrentState currentState)
        {
            switch (currentState)
            {
                case CurrentState.Idle:
                    _sJManager.anim.Play(AnimHash.Idle);
                    break;
                case CurrentState.Normal:
                    _sJManager.barrel.SetActive(false);
                    _sJManager.anim.Play(AnimHash.NormalWalk);
                    break;
                case CurrentState.WithBarrel:
                    _sJManager.anim.Play(AnimHash.WalkWithBarrel);
                    _sJManager.barrel.SetActive(true);
                    break;

            }
        }
        public void MoveToNextWaypoint(int currentWaypointIndex)
        {
            Transform target = _sJManager.waypointBank.path[currentWaypointIndex];
            _sJManager.agent.SetDestination(target.position);
        }

    }
}