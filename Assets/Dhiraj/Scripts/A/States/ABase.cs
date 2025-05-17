using Unity.VisualScripting;
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
            public const string Punch_0 = "Punch";
            public const string Punch_1 = "Punch 0";
            public const string Punch_2 = "Punch 1";
            public const string Stuned = "Stun";
            public const string PunchCount = "PunchCount";
            public const string PunchTrigger = "PunchTrigger";



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
            Drinking = 4,
            Stun = 5,
            notWalking= 6
        }

        protected AManager _aManager;
        protected float distanceFromTarget;
        protected float hitReactTimer;

        protected float attackCooldown = 0.2f;
        protected float attackTimer = 0f;

        protected bool isHitReacting;
        protected bool isStunned;


        public virtual void StartState()
        {

        }
        public virtual void UpdateState()
        {
            
            if (_aManager.enemyTarget)
            {                
                _aManager.ChangeState(_aManager.aCombatStance);
                return;
            }
            else
            {
                _aManager.ChangeState(_aManager.aIdle);
            }
        }

        public virtual void EndState()
        {

        }


        public void ChangeAnimationState(CurrentState currentState)
        {
            switch (currentState)
            {
                case CurrentState.Walking:
                    _aManager.anim.Play(AnimHash.Walking);                 
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

        protected readonly string[] attackAnimations = {
            AnimHash.Punch_0,
            AnimHash.Punch_1,
            AnimHash.Punch_2
        };

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

        protected float GetAnimationLenght(string AnimationName)
        {
            if (_aManager.anim.runtimeAnimatorController == null)
            {
                return 0;
            }

            AnimationClip[] clips = _aManager.anim.runtimeAnimatorController.animationClips;

            foreach (AnimationClip clip in clips)
            {
                if (clip.name.Contains(AnimationName))
                {
                    return clip.length;
                }
            }
            return 1;
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

        public void OnHit(bool isTrue)
        {
            isHitReacting = isTrue;
            hitReactTimer = 0f;
            _aManager.isPushBack = true;
            Debug.Log("On hit called");
        }

        public void OnStunned(bool isTrue)
        {
            isStunned = isTrue;
        }

        public void RotatePlayer()
        {
            Vector3 dir = (_aManager.enemyTarget.position - _aManager.transform.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
            _aManager.transform.rotation = Quaternion.Slerp(_aManager.transform.rotation, lookRot, Time.deltaTime * 5f);
        }
    }
}

