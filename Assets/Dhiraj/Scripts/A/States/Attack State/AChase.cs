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
            Debug.Log("Chase Started");
        }

        public override void UpdateState()
        {
            //base.UpdateState();
            distanceFromTarget = Vector3.Distance(_aManager.transform.position, _aManager.enemyTarget.position);

            if (distanceFromTarget < 1.1f)
            {
                _aManager.ChangeState(_aManager.aCombatStance);
            }
            else if(!_aManager.isPushBack)
            {
                _aManager.agent.isStopped = false;
                _aManager.agent.SetDestination(_aManager.enemyTarget.position);
            }         
        }

        public override void EndState()
        {
            base.EndState();
            Debug.Log("Chase Ended");
            if (!_aManager.isPushBack) _aManager.agent.isStopped = true;
            _aManager.anim.SetBool(AnimHash.Walking, false);



        }
    }
}
