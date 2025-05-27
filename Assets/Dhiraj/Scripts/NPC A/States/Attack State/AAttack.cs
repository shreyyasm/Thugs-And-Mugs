using UnityEngine;

namespace Dhiraj
{

    public class AAttack : ABase
    {
        int index;
        float waitTime = 0;
        float waitCurrentTime = 0;
        bool isAttacked = false;

        float waitTimer = 0;
        public AAttack(AManager aManager) : base(aManager)
        {
            _aManager = aManager;
        }

        public override void StartState()
        {
            //base.StartState();
            index = Random.Range(0, 3);
            waitTime = GetAnimationLenght(attackAnimations[index] + 1.5f);
            waitCurrentTime = waitTime;
            isAttacked = false;
            waitTimer = Random.Range(0.5f, 1f);
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

            waitTimer -= Time.deltaTime;
            if (waitTimer > 0.1f) return;
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
