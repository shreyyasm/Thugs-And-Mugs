using UnityEngine;

namespace Dhiraj
{

    public class AChase : ABase
    {
        public AChase(AManager aManager) : base(aManager)
        {
            _aManager = aManager;
        }

        public override void StartState()
        {
            base.StartState();
            
            _aManager.anim.SetBool(AnimHash.Walking, true);

            //Debug.Log("Chase Started");
        }

        public override void UpdateState()
        {
            //base.UpdateState();

            if (!_aManager.enemyTarget)
            {
                _aManager.ChangeState(_aManager.aIdle);
                return;
            }
            distanceFromTarget = Vector3.Distance(_aManager.transform.position, _aManager.enemyTarget.position);

            if (distanceFromTarget < 1.1f)
            {
                _aManager.ChangeState(_aManager.aCombatStance);
            }
            else if(!_aManager.isPushBack)
            {
                //_aManager.agent.isStopped = false;
                // _aManager.agent.SetDestination(_aManager.enemyTarget.position);

                MoveToTarget(_aManager.enemyTarget);

            }


            if (!_aManager.enemyTarget)
            {
                if (!_aManager.seat)
                {
                    _aManager.ChangeState(_aManager.aLookAround);
                }
                else
                {
                    _aManager.ChangeState(_aManager.aLocomotion);
                }
            }
        }

        public override void EndState()
        {
            base.EndState();
            //Debug.Log("Chase Ended");
            _aManager.anim.SetBool(AnimHash.Walking, false);
        }

        void MoveToTarget(Transform target)
        {
            if (_aManager.enemyTarget == null) return;

            Vector3 direction = (target.position - _aManager.transform.position).normalized;
            float distance = Vector3.Distance(_aManager.transform.position, _aManager.enemyTarget.position);

            if (distance > _aManager.agent.stoppingDistance)
            {
                _aManager.transform.position += direction * 1 * Time.deltaTime;

                // Optional: rotate towards target
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                _aManager.transform.rotation = Quaternion.Slerp(_aManager.transform.rotation, lookRotation, 100 * Time.deltaTime);
            }
        }
    }
}
