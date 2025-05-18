/*using UnityEngine;


namespace Dhiraj
{
    public class AAttacking : ABase
    {
        private float attackCooldown = 0.6f;
        private float attackTimer = 0f;
        private bool isStunned = false;
        private float stunTimer = 0f;
        private float stunDuration = 1f;
        private bool knockedOut = false;
        private bool canAttack = true;

        private bool isHitReacting = false;
        private float hitReactTimer = 0f;
        private float hitReactDuration = 0.4f;

        private bool justArrivedToAttackRange = false;

        public AAttacking(AManager aManager) : base(aManager)
        {
            _aManager = aManager;
        }

        public override void StartState()
        {
            base.StartState();

            _aManager.agent.isStopped = true;
            attackTimer = 0f;
            isStunned = false;
            knockedOut = false;
            canAttack = true;

            _aManager.anim.SetBool("CombatStance", true);
        }

        public override void UpdateState()
        {
            if (knockedOut || _aManager.enemyTarget == null)
                return;

            if (isHitReacting)
            {
                hitReactTimer += Time.deltaTime;
                if (hitReactTimer >= hitReactDuration)
                {
                    isHitReacting = false;
                    hitReactTimer = 0f;
                }
                return; // ✅ Prevent movement and attacking during hit recovery
            }

            float distanceToTarget = Vector3.Distance(_aManager.transform.position, _aManager.enemyTarget.position);
            if (_aManager.isLog) Debug.Log($"Stopping Distance =>{_aManager.agent.stoppingDistance} || Current Distance => {distanceToTarget}");

            if (distanceToTarget > _aManager.agent.stoppingDistance)
            {
                if (_aManager.agent.isStopped)
                {
                    _aManager.agent.isStopped = false;
                }

                _aManager.agent.SetDestination(_aManager.enemyTarget.position);
                if (_aManager.isLog) Debug.Log("Error is coming from here");
                // ✅ Only trigger walking animation if not already playing it
                AnimatorStateInfo state = _aManager.anim.GetCurrentAnimatorStateInfo(0);
                if (!state.IsName("Walking"))
                {
                    _aManager.anim.SetBool("Walking", true);
                    if (_aManager.isLog) Debug.Log("Error is coming from here for walking");
                }

                _aManager.anim.SetBool("CombatStance", false);
                canAttack = false;
                justArrivedToAttackRange = true;
                return;
            }
            else
            {
                if (!_aManager.agent.isStopped)
                {
                    _aManager.agent.isStopped = true;
                    justArrivedToAttackRange = true;
                }

                _aManager.anim.SetBool("Walking", false);
                _aManager.anim.SetBool("CombatStance", true);
                canAttack = true;
            }

            // ✅ Face the target
            Vector3 dir = (_aManager.enemyTarget.position - _aManager.transform.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
            _aManager.transform.rotation = Quaternion.Slerp(_aManager.transform.rotation, lookRot, Time.deltaTime * 5f);

            // ✅ Handle stun logic
            if (isStunned)
            {
                stunTimer += Time.deltaTime;
                if (stunTimer >= stunDuration)
                    RecoverFromStun();
                return;
            }

            // ✅ Attack logic
            attackTimer += Time.deltaTime;

            if ((attackTimer >= attackCooldown || justArrivedToAttackRange) && canAttack && !IsInAttackAnimation())
            {
                PerformAttack();
                attackTimer = 0f;
                justArrivedToAttackRange = false;
            }
        }

        public void OnHit()
        {
            isHitReacting = true;
            hitReactTimer = 0f;
            Debug.Log("On hit called");
        }

        private bool IsInAttackAnimation()
        {
            AnimatorStateInfo stateInfo = _aManager.anim.GetCurrentAnimatorStateInfo(0); // base layer

            string[] punchStates = { "Punch", "Punch 0", "Punch 1", "Punch 2", "Punch 3", "Hit", "Stun", "Walking" };

            foreach (var state in punchStates)
            {
                if (stateInfo.IsName(state))
                    return true;
            }

            return false;
        }


        private void PerformAttack()
        {
            if (_aManager.isLog) Debug.Log("Attack Perform => ");
            int index = Random.Range(0, 5);
            PlayAttackAnimation(index);
            //_aManager.anim.SetTrigger("Attack");

            AManager targetManager = _aManager.enemyTarget.GetComponent<AManager>();
            if (targetManager != null)
            {
                //targetManager.ReceiveDamage(5, _aManager);

                // Self-check for damage too if needed for simulation/testing
                ReceiveDamage(Random.Range(5, 10));
            }
        }

        public void ReceiveDamage(int damage)
        {
            if (knockedOut) return;

            //_aManager.currentHP -= damage;

            if (isStunned)
            {
                KnockOut();
                return;
            }

            if (_aManager.currentHP <= 0)
            {
                KnockOut();
            }
            else if (_aManager.currentHP <= _aManager.maxHP / 2 && !isStunned)
            {
                Stun();
            }
        }

        private void Stun()
        {
            isStunned = true;
            stunTimer = 0f;
            canAttack = false;
            //_aManager.anim.SetTrigger("Stun");
            //ChangeAnimationState(CurrentState.Stun);
            Debug.Log("NPC stunned!");
        }

        private void RecoverFromStun()
        {
            isStunned = false;
            canAttack = true;
            Debug.Log("Recovered from stun");
        }

        private void KnockOut()
        {
            knockedOut = true;
            _aManager.anim.SetTrigger("KnockOut");
            _aManager.ChangeState(_aManager.aEnd);
            Debug.Log("NPC knocked out");
        }

        public override void EndState()
        {
            base.EndState();
            _aManager.agent.isStopped = false;
            _aManager.anim.SetBool("CombatStance", false);
        }
    }
}
*/