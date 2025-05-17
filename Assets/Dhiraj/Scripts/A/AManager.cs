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
        public Collider[] thisCollider;
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
        public bool isDead = false;
        public bool isActiveInCombat = false;
        public bool IsAlive() => !isDead;

        //Attack settings
        [Header("Attack Settings")]
        public Vector2 forceRange = new Vector2(1f, 3f);

        public float forceDuration;
        private float pushCooldownTimer = 0f;
        private const float pushCooldownDuration = 0.5f; // half a second cooldown

        // Enemy & HP System
        public List<AManager> enemyTargets = new List<AManager>();
        public Transform enemyTarget
        {
            get
            {
                enemyTargets.RemoveAll(e => e == null || e.Equals(null) || !e.IsAlive());
                return enemyTargets.Count > 0 ? enemyTargets[0].transform : null;
            }
        }

        public ParticleSystem bloodParticalSystem;
        public ParticleSystem stunnParticalSystem;
        public Transform impactPosition;

        public bool isPlayer = false;
        public int maxHP = 100;
        public int currentHP = 100;
        public int dealDamageAmount = 5;


        private void Start()
        {
            Initiaized();           
        }


        private Vector3 lastPosition;
        private Vector3 movementDirection;
        private void Update()
        {
            currentState.UpdateState();
            CurrentState = currentState.ToString();
            CheckAndAssignNewTarget();

            if (pushCooldownTimer > 0f)
                pushCooldownTimer -= Time.deltaTime;

            movementDirection = (transform.position - lastPosition).normalized;
            lastPosition = transform.position;
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
                Die();
                //Destroy(this.gameObject);
            }
        }

        public void SortEnemyTargets()
        {
            enemyTargets = enemyTargets.OrderBy(e => e.currentHP).ToList();
        }

        public void Die()
        {
            //Is dead now
            anim.Play("Stun");
            isDead = true;

            CrowdControlManager.Instance.Unregister(this);
            
            aController.Disable();

            foreach (var target in thisCollider)
            {
                target.enabled = false;
            }
            stunnParticalSystem.Play();
            Debug.Log(this.name + " NPC is dead");
            //Destroy(this);

        }
        public bool IsFighting(AManager other)
        {
            return enemyTargets.Contains(other);
        }
        public void CheckAndAssignNewTarget()
        {
            if (!isActiveInCombat) return;

            enemyTargets.RemoveAll(e => e == null || !e.IsAlive());

            if (enemyTargets.Count == 0)
            {
                AManager newEnemy = CrowdControlManager.Instance.GetNewEnemyFor(this);

                if (newEnemy != null)
                {
                    enemyTargets.Add(newEnemy);
                    //Debug.Log($"{name} assigned new enemy: {newEnemy.name}");
                }
            }
        }
        public void HitAnimation()
        {            
            // We allow interrupting walking or idle with a Hit animation
            anim.SetTrigger("Hit");

            // Lock movement temporarily
            StartCoroutine(ReenableControlAfterHit(0.3f));
        }

        private IEnumerator ReenableControlAfterHit(float delay)
        {
            aController.Disable();
            yield return new WaitForSeconds(delay);
            aController.Enable(); // Assuming this method exists
        }

        public void ApplyPushback(Vector3 direction, float force, float duration)
        {
            if (!gameObject.activeInHierarchy || isPushBack || pushCooldownTimer > 0f)
            {
                return;
            }

            pushCooldownTimer = pushCooldownDuration;
            StartCoroutine(HandlePushback(direction, force, duration));
            isPushBack = true;
        }

        private IEnumerator HandlePushback(Vector3 direction, float force, float duration)
        {
            // Stop movement & disable controller temporarily
            agent.enabled = false;
            aController.Disable(); // Disable state update

            float timer = 0f;
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = startPosition + direction.normalized * force;

            float pushRadius = 0.5f; // How wide the push detection should be
            LayerMask npcLayer = LayerMask.GetMask("NPC"); // Ensure your NPCs are in this layer

            while (timer < duration)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, timer / duration);
                timer += Time.deltaTime;

                // Detect other NPCs in the path
                Collider[] hits = Physics.OverlapSphere(transform.position, pushRadius, npcLayer);
                foreach (var hit in hits)
                {
                    if (hit.transform == this.transform) continue;

                    AManager otherManager = hit.GetComponentInParent<AManager>();
                    if (otherManager != null && !otherManager.isPushBack)
                    {                       
                        otherManager.isPushBack = true;
                        // Start pushback on collided NPC with reduced force
                        otherManager.StartCoroutine(otherManager.HandlePushback(-direction, force * 0.7f, duration * 0.9f));
                    }
                }

                yield return null;
            }

            transform.position = targetPosition;

            yield return new WaitForSeconds(0.1f); // brief delay

            aController.Enable();
            isPushBack = false;
        }



        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("NPCHitBox"))
            {
                AManager otherManager = other.GetComponentInParent<AManager>();

                //Debug.Log($"I'm {this.name} || Hitbox : {otherManager.name}");
                bloodParticalSystem.Play();
                if (otherManager != null && otherManager != this)
                {
                    CrowdControlManager.Instance.TryRegister(this);
                    CrowdControlManager.Instance.TryRegister(otherManager);

                    this.isActiveInCombat = true;
                    otherManager.isActiveInCombat = true;

                    if (!enemyTargets.Contains(otherManager))
                    {
                        enemyTargets.Add(otherManager);
                        SortEnemyTargets();

                    }
                    ReceiveDamage(dealDamageAmount, otherManager);
                    HitAnimation();
                }
               
            }

            if (other.CompareTag("NPC"))
            {
                AManager otherManager = other.GetComponentInParent<AManager>();

                if (otherManager != null && otherManager != this)
                {
                    CrowdControlManager.Instance.TryRegister(this);
                    CrowdControlManager.Instance.TryRegister(otherManager);

                    this.isActiveInCombat = true;
                    otherManager.isActiveInCombat = true;

                    if (!enemyTargets.Contains(otherManager))
                    {
                        enemyTargets.Add(otherManager);
                        SortEnemyTargets();
                        //Debug.Log($"{name} added enemy: {otherManager.name}");
                    }
                }


                //Vector3 pushDir = (enemyTarget.position - transform.position).normalized;
                Vector3 pushDir = movementDirection.sqrMagnitude > 0.001f ? movementDirection : transform.forward;
                //Vector3 pushDir = transform.forward.normalized;
                float pushForce = Random.Range(forceRange.x, forceRange.y);
                float pushDuration = forceDuration;

                otherManager.ApplyPushback(pushDir, pushForce, pushDuration);
                //Debug.Log($"{otherManager.gameObject.name} => {otherManager.currentHP}");
                return;
            }
        }
    }
    

}
