using UnityEngine;

namespace Dhiraj
{

    public class ACombatStance : ABase
    {
        public ACombatStance(AManager aManager) : base(aManager)
        {
            _aManager = aManager;
        }

        public override void StartState()
        {
            //base.StartState();
            
            _aManager.anim.SetBool("CombatStance", true);
            attackTimer = 0;            
            Debug.Log($"Steps Attack Cooldown :> {attackCooldown}");
        }

        public override void UpdateState()
        {
            //base.UpdateState();      
            if (_aManager.isPushBack)
            {
                return;
            }

            _aManager.agent.isStopped = true;
            distanceFromTarget = Vector3.Distance(_aManager.transform.position, _aManager.enemyTarget.position);

            if (distanceFromTarget > 1.1f)
            {
                _aManager.ChangeState(_aManager.aChase);
                
            }
            else if(distanceFromTarget < 1.1f)
            {
                attackTimer += Time.deltaTime;
                Debug.Log($"Steps Attack Timer:> {attackTimer}");
                if (attackTimer > attackCooldown)
                {
                    _aManager.ChangeState(_aManager.aAttack);
                    
                    _aManager.isAttack = true;
                }

            }

            Vector3 dir = (_aManager.enemyTarget.position - _aManager.transform.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
            _aManager.transform.rotation = Quaternion.Slerp(_aManager.transform.rotation, lookRot, Time.deltaTime * 5f);

        }

        public override void EndState()
        {
            base.EndState();
            _aManager.anim.SetBool("CombatStance", false);
            if (!_aManager.isPushBack) _aManager.agent.isStopped = false;
        }
    }
}
