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
            _aManager.agent.enabled = false;
            _aManager.anim.SetBool("CombatStance", true);
            attackTimer = 0;            
        }

        public override void UpdateState()
        {
            //base.UpdateState();      
            if (!_aManager.enemyTarget)
            {
                _aManager.ChangeState(_aManager.aIdle);
                return;
            }
            if (_aManager.isPushBack)
            {
                return;
            }

            distanceFromTarget = Vector3.Distance(_aManager.transform.position, _aManager.enemyTarget.position);

            if (distanceFromTarget > 1.1f)
            {
                _aManager.ChangeState(_aManager.aChase);
                
            }
            else if(distanceFromTarget < 1.1f)
            {
                attackTimer += Time.deltaTime;
                

                if (attackTimer > attackCooldown)
                {
                    _aManager.ChangeState(_aManager.aAttack);                    
                    _aManager.isAttack = true;
                }

            }
        }

        public override void EndState()
        {
            base.EndState();
            _aManager.anim.SetBool("CombatStance", false);
        }
    }
}
