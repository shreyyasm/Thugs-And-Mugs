using UnityEngine;

namespace Dhiraj
{
    public class AAttacking : ABase
    {
        private float attackCooldown = 1.5f;
        private float attackTimer = 0f;
        private bool isAttacking = false;
        private float stoppingDistance = 1.8f;
        private string[] attackAnimNames = new string[] { "Punch", "Punch 0", "Punch 1", "Punch 2" };

        public AAttacking(AManager aManager) : base(aManager)
        {
            _aManager = aManager;
        }

        public override void StartState()
        {
            base.StartState();
            _aManager.agent.isStopped = false;
            _aManager.aController.Disable(); // Make sure weapon is off
            isAttacking = false;
            attackTimer = 0f;

            ChangeAnimationState(CurrentState.Idle); // Combat idle stance
        }

        public override void UpdateState()
        {
            base.UpdateState();

            if (_aManager.enemyTarget == null || _aManager.currentHP <= 0)
            {
                _aManager.ChangeState(_aManager.aIdle);
                return;
            }

            AnimatorStateInfo anim = _aManager.anim.GetCurrentAnimatorStateInfo(0);
            if (anim.IsName("Hit") || anim.IsName("Stun"))
            {
                _aManager.aController.Disable(); // Force cancel
                isAttacking = false;
                return;
            }

            float distance = Vector3.Distance(_aManager.transform.position, _aManager.enemyTarget.position);
            if (distance > stoppingDistance)
            {
                _aManager.agent.SetDestination(_aManager.enemyTarget.position);
                ChangeAnimationState(CurrentState.Walking);
                return;
            }

            _aManager.agent.ResetPath();
            _aManager.transform.LookAt(_aManager.enemyTarget);

            if (!isAttacking && attackTimer <= 0f)
            {
                Attack();
            }

            if (attackTimer > 0f)
                attackTimer -= Time.deltaTime;
        }

        private void Attack()
        {
            isAttacking = true;
            attackTimer = attackCooldown;

            int index = Random.Range(0, attackAnimNames.Length);
            _aManager.anim.SetTrigger(attackAnimNames[index]);

            // Activate hitbox shortly after (simulate wind-up)
            _aManager.StartCoroutine(AttackSequence());
        }

        private System.Collections.IEnumerator AttackSequence()
        {
            yield return new WaitForSeconds(0.3f); // wind-up
            _aManager.aController.Enable();

            yield return new WaitForSeconds(0.2f); // active window
            _aManager.aController.Disable();

            yield return new WaitForSeconds(attackCooldown - 0.5f); // remaining cooldown
            isAttacking = false;
        }

        public override void EndState()
        {
            base.EndState();
            _aManager.agent.ResetPath();
            _aManager.aController.Disable();
            isAttacking = false;
        }

        public void OnHit()
        {
            // If hit externally
            isAttacking = false;
            _aManager.aController.Disable();
        }
    }
}
