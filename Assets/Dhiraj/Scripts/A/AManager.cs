using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;

namespace Dhiraj
{
    public class AManager : MonoBehaviour
    {
        #region State Initialize
        public ABase currentState;
        public AIdle aIdle;
        public ALocomotion aLocomotion;
        public ALookAround aLookAround;
        public AActions aActions;
        public ADrinking aDrinking;
        public ACombatStance aCombatStance;
        public AAttack aAttack;
        public AStunn aStunn;
        public AChase aChase;
        public AEnd aEnd;
        #endregion

        public string CurrentState;
        public AController aController;
        public WaypointBank waypointBank;
        public NavMeshAgent agent;
        public Animator anim;
        public Seat seat;

        public GameObject mug;
        public Image fillImage;
        public CanvasGroup sliderGroup;

        public bool isLog = false;

        public bool GoReturn = false;
        public bool isLookAround = false;
        public bool isPushBack = false;
        public bool isAttack = false;

        //Attack settings
        [Header("Attack Settings")]
        public Vector2 forceRange = new Vector2(1f, 3f);
        public Vector2 forceDuration = new Vector2(1f, 3f);


        // Enemy & HP System
        public List<AManager> enemyTargets = new List<AManager>();
        public Transform enemyTarget
        {
            get
            {
                // Clean up destroyed or null entries
                enemyTargets.RemoveAll(e => e == null || e.Equals(null));

                return enemyTargets.Count > 0 ? enemyTargets[0].transform : null;
            }
        }

        public bool isPlayer = false;
        public int maxHP = 100;
        public int currentHP = 100;
        public int dealDamageAmount = 5;


        private void Start()
        {
            Initiaized();
        }
        private void Update()
        {
            currentState.UpdateState();
            CurrentState = currentState.ToString();
        }
        public void Initiaized()
        {
            aIdle = new AIdle(this);
            aLocomotion = new ALocomotion(this);
            aActions = new AActions(this);
            aDrinking = new ADrinking(this);
            aLookAround = new ALookAround(this);

            aCombatStance = new ACombatStance(this);
            aAttack = new AAttack(this);
            aStunn = new AStunn(this);
            aChase = new AChase(this);

            aEnd = new AEnd(this);

            currentState = aIdle;
            currentState.StartState();
        }
        public void ChangeState(ABase newState)
        {
            currentState.EndState();
            currentState = newState;
            currentState.StartState();
        }

        public void ReceiveDamage(int damage, AManager attacker)
        {
            if (currentHP <= 0) return;

            currentHP -= damage;

            if (attacker != null && attacker != this && !enemyTargets.Contains(attacker))
            {
                enemyTargets.Add(attacker);
                SortEnemyTargets();
            }

            // Clean up dead enemies
            enemyTargets.RemoveAll(e => e == null || e.currentHP <= 0);

            if (currentHP <= 0)
            {
                ChangeState(aStunn);
                //Destroy(this.gameObject);
            }
        }

        public void SortEnemyTargets()
        {
            enemyTargets = enemyTargets
                .OrderByDescending(e => e.isPlayer)
                .ThenBy(e => Vector3.Distance(transform.position, e.transform.position))
                .ToList();
        }


        public void HitAnimation()
        {
            // Skip if already in punch or hit state
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            string[] blockStates = { "Punch", "Punch 0", "Punch 1", "Punch 2", "Hit", "Stun" };

            foreach (string stateName in blockStates)
            {
                if (stateInfo.IsName(stateName))
                {
                    // Already playing a reaction or attack, don't interrupt
                    return;
                }
            }

            // We allow interrupting walking or idle with a Hit animation
            anim.ResetTrigger("Hit");
            anim.SetTrigger("Hit");

            // Lock movement temporarily
            StartCoroutine(ReenableControlAfterHit(0.3f));
            currentState.OnHit(false); // Lock movement and attack
        }

        private IEnumerator ReenableControlAfterHit(float delay)
        {
            aController.Disable();
            yield return new WaitForSeconds(delay);
            aController.Enable(); // Assuming this method exists
        }

        public void ApplyPushback(Vector3 direction, float force, float duration)
        {
            if (!gameObject.activeInHierarchy) return;
            StartCoroutine(HandlePushback(direction, force, duration));
            return;
        }

        private IEnumerator HandlePushback(Vector3 direction, float force, float duration)
        {
            // Stop movement & disable controller temporarily            
            agent.enabled = false;
            aController.Disable(); // Disable state update

            float timer = 0f;
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = startPosition + direction.normalized * force;

            while (timer < duration)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, timer / duration);
                timer += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPosition;

            yield return new WaitForSeconds(0.1f); // brief delay

            // Re-enable
            agent.enabled = true;
            aController.Enable();
            isPushBack = false;
        }



        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("NPCHitBox"))
            {
                AManager otherManager = other.GetComponentInParent<AManager>();

                if (otherManager != null && otherManager != this && !enemyTargets.Contains(otherManager))
                {
                    enemyTargets.Add(otherManager);
                    SortEnemyTargets(); // Optional, if you want to keep priorities sorted
                    Debug.Log($"Added new enemy: {otherManager.name} || {enemyTarget.name}");
                }
            }

            if (other.CompareTag("NPC"))
            {
                AManager otherManager = other.GetComponentInParent<AManager>();

                if (otherManager != null && otherManager != this && !enemyTargets.Contains(otherManager))
                {
                    enemyTargets.Add(otherManager);
                    SortEnemyTargets(); // Optional, if you want to keep priorities sorted
                    Debug.Log($"Added new enemy: {otherManager.name} || {enemyTarget.name}");
                }


                otherManager.ReceiveDamage(dealDamageAmount, otherManager);
                otherManager.HitAnimation();
                Vector3 pushDir = (enemyTarget.position - transform.position).normalized;
                float pushForce = Random.Range(forceRange.x, forceRange.y);
                float pushDuration = Random.Range(forceDuration.x, forceDuration.y);

                otherManager.ApplyPushback(pushDir, pushForce, pushDuration);
                //Debug.Log($"{otherManager.gameObject.name} => {otherManager.currentHP}");
                return;
            }
        }
    }
    

}
