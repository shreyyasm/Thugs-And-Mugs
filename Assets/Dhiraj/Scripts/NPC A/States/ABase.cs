using UnityEngine;

namespace Dhiraj
{
    public abstract class ABase
    {
        protected AManager _aManager;

        protected float distanceFromTarget;
        protected float attackCooldown = 0.2f;
        protected float attackTimer = 0f;

        protected readonly string[] attackAnimations = {
            AnimHash.Punch_0,
            AnimHash.Punch_1,
            AnimHash.Punch_2
        };

        public ABase(AManager aManager)
        {
            _aManager = aManager;
        }

        #region State Control

        public virtual void StartState() { }

        public virtual void UpdateState()
        {
            if (_aManager.enemyTarget)
            {
                _aManager.ChangeState(_aManager.aCombatStance);
            }
        }

        public virtual void EndState() { }

        #endregion

        #region Animation Helpers

        public enum CurrentState
        {
            Idle = 0,
            Walking = 1,
            LookingAround = 2,
            Waiting = 3,
            Drinking = 4,
            Stun = 5,
            notWalking = 6
        }

        public void ChangeAnimationState(CurrentState currentState)
        {
            switch (currentState)
            {
                case CurrentState.Walking:
                    _aManager.anim.Play(AnimHash.Walk);
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
                case CurrentState.Stun:
                    _aManager.anim.Play(AnimHash.Stuned);
                    _aManager.anim.SetBool(AnimHash.Stuned, true);
                    break;
            }
        }

        public void PlayAttackAnimation(int randomNum)
        {
            if (randomNum >= 0 && randomNum < attackAnimations.Length)
            {
                _aManager.anim.SetTrigger(attackAnimations[randomNum]);
            }
            else
            {
                Debug.LogWarning($"Invalid attack animation index: {randomNum}");
            }
        }

        protected float GetAnimationLenght(string animationName)
        {
            if (_aManager.anim.runtimeAnimatorController == null)
                return 0;

            foreach (var clip in _aManager.anim.runtimeAnimatorController.animationClips)
            {
                if (clip.name.Contains(animationName))
                    return clip.length;
            }

            return 1;
        }

        #endregion

        #region Movement

        public void MoveToNextWaypoint(int currentWaypointIndex)
        {
            Transform target = _aManager.waypointBank.path[currentWaypointIndex];
            if(!_aManager.agent.isStopped) _aManager.agent.SetDestination(target.position);
            ChangeAnimationState(CurrentState.Walking);
        }

        public void MoveToWaypoint(Vector3 pos)
        {
            if (!_aManager.agent.isStopped) _aManager.agent.SetDestination(pos);
            ChangeAnimationState(CurrentState.Walking);
        }

        public void RotatePlayer()
        {
            Vector3 dir = (_aManager.enemyTarget.position - _aManager.transform.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
            _aManager.transform.rotation = Quaternion.Slerp(_aManager.transform.rotation, lookRot, Time.deltaTime * 5f);
        }

        #endregion

        #region Animation Constants

        public class AnimHash
        {
            public const string Idle = "Idle";
            public const string LookingAround = "Looking Around";
            public const string SitAndDrinking = "Sit and Drinking";
            public const string SittingWaiting = "Sitting Waiting";
            public const string Walking = "Walking";
            public const string Walk = "Walk";
            public const string Punch_0 = "Punch";
            public const string Punch_1 = "Punch 0";
            public const string Punch_2 = "Punch 1";
            public const string Stuned = "Stun";
            public const string PunchCount = "PunchCount";
            public const string PunchTrigger = "PunchTrigger";
        }

        #endregion
    }
}
