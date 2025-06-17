using UnityEngine;

namespace Dhiraj
{

    public class ACombatStance : ABase
    {
        public ACombatStance(AManager aManager) : base(aManager)
        {
            _aManager = aManager;
        }
        public float attackDistance = 1.1f;

        public override void StartState()
        {
            //base.StartState();
            _aManager.agent.enabled = false;
            _aManager.anim.SetBool("CombatStance", true);
            attackTimer = 0;
            GameManager.Instance.isFightStarted = true;

            switch (_aManager.npcType)
            {
                case NPCType.Pushpa:
                    attackDistance = 1.1f;
                    break;
                case NPCType.Warrior:
                    attackDistance = 1.1f;
                    break;
                case NPCType.Outlaw:
                    attackDistance = 3f;
                    break;
                case NPCType.Prisonar:
                    attackDistance = 1.5f;
                    break;
            }

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

            if (distanceFromTarget > attackDistance)
            {
                _aManager.ChangeState(_aManager.aChase);
                
            }
            else if(distanceFromTarget < attackDistance)
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
