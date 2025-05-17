using UnityEngine;

namespace Dhiraj
{

    public class AAttack : ABase
    {
        int index;
        float waitTime = 0;
        float waitCurrentTime = 0;
        bool isAttacked = false;
        public AAttack(AManager aManager) : base(aManager)
        {
            _aManager = aManager;
        }

        public override void StartState()
        {
            //base.StartState();
            index = Random.Range(0, 3);            
            waitTime = GetAnimationLenght(attackAnimations[index]);
            waitCurrentTime = waitTime;
            isAttacked = false;
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
            RotatePlayer();

            if (distanceFromTarget < 1.1f && !isAttacked)
            {                
                PlayAttackAnimation(index);
              
                isAttacked = true;
            }
            waitCurrentTime -= Time.deltaTime;
            if (waitCurrentTime <= 0)
            {
                _aManager.ChangeState(_aManager.aCombatStance);
            }

            if (distanceFromTarget > 1.1f && waitCurrentTime <= waitTime/2)
                _aManager.ChangeState(_aManager.aCombatStance);
        }

        public override void EndState()
        {
            base.EndState();
            _aManager.isAttack = false;
        }
    }
}
