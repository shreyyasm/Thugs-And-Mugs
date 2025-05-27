using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace Dhiraj
{
    public enum NPCType { 
    Pushpa = 0,
    Warrior = 1,
    Outlaw = 2,
    Prisonar = 3    
    }

    public class AManager : MonoBehaviour
    {        
        public NPCType npcType;
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

        #region Components
        [Header("Components")]
        public AController aController;
        public WaypointBank waypointBank;
        public NPCSpawnManager npcSpawnManager;
        public NavMeshAgent agent;
        public Collider[] thisCollider;
        public Animator anim;
        public Seat seat;
        public GameObject mug;
        public Image fillImage;
        public CanvasGroup sliderGroup;
        public ParticleSystem bloodParticalSystem;
        public ParticleSystem stunnParticalSystem;
        public Transform impactPosition;
        public Rigidbody rb;
        #endregion

        #region State Flags
        [Header("State Flags")]
        public bool canStartFight = false;
    
        [Space(5)]
        public bool isLog = false;
        public bool GoReturn = false;
        public bool isLookAround = false;
        public bool isPushBack = false;
        public bool isAttack = false;
        public bool isDead = false;
        public bool isActiveInCombat = false;
        public bool isPlayer = false;

        [Space(5)]
        public bool isServed = false;
        public bool playerInteracting = false;

        public bool IsAlive() => !isDead;
        #endregion

        #region Health & Combat
        [Header("Health & Combat")]
        public int maxHP = 100;
        public int currentHP = 100;
        public int dealDamageAmount = 5;
        public List<AManager> enemyTargets = new List<AManager>();
        public Transform enemyTarget => enemyTargets
            .Where(e => e != null && e.IsAlive())
            .Select(e => e.transform)
            .FirstOrDefault();
        #endregion

        #region Attack Settings
        [Header("Attack Settings")]
        public Vector2 forceRange = new Vector2(1f, 3f);
        public float forceDuration;
        private float pushCooldownTimer = 0f;
        private const float pushCooldownDuration = 0.5f;
        #endregion

        private Vector3 lastPosition;
        private Vector3 movementDirection;
        public string CurrentState;

        private void Start() => Initiaized();

        private void Update()
        {
            currentState.UpdateState();
            CurrentState = currentState.ToString();

            CheckAndAssignNewTarget();
            if (!isActiveInCombat)
                return;

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

            enemyTargets.RemoveAll(e => e == null || e.currentHP <= 0);

            if (currentHP <= 0)
            {
                ChangeState(aStunn);
                Die();
            }
        }

        public void Die()
        {
            if (!isDead)
            {
                Destroy(this.gameObject, 5);
                isDead = true;
            }
            anim.Play("Stun");

            CrowdControlManager.Instance.Unregister(this);
            aController.Disable();

            foreach (var col in thisCollider)
                col.enabled = false;

            stunnParticalSystem.Play();
            rb.isKinematic = true; // Disable physics interactions
            Debug.Log($"{name} NPC is dead");
        }
        public void Death()
        {
            DestroyImmediate(this.gameObject);
        }
        public void SortEnemyTargets() =>
            enemyTargets = enemyTargets.OrderBy(e => e.currentHP).ToList();

        public bool IsFighting(AManager other) => enemyTargets.Contains(other);

        public void CheckAndAssignNewTarget()
        {
            if (!isActiveInCombat) return;

            enemyTargets.RemoveAll(e => e == null || !e.IsAlive());

            if (enemyTargets.Count == 0)
            {
                AManager newEnemy = CrowdControlManager.Instance.GetNewEnemyFor(this);
                if (newEnemy != null)
                    enemyTargets.Add(newEnemy);
            }
        }

        public void HitAnimation()
        {
            anim.SetTrigger("Hit");
        }

        public void ApplyPushback(Vector3 direction, float force, float duration)
        {
            if (!gameObject.activeInHierarchy || isPushBack || pushCooldownTimer > 0f)
                return;

            isPushBack = true;
            pushCooldownTimer = pushCooldownDuration;

            // Disable NavMeshAgent so physics takes over
            agent.enabled = false;
            aController.Disable();

            // Apply physics push
           // rb.isKinematic = false; // ensure it's affected by physics
            rb.AddForce(direction.normalized * force, ForceMode.Impulse);

            // Schedule re-enabling control
            StartCoroutine(ResetAfterPush(duration));
        }

        private IEnumerator ResetAfterPush(float delay)
        {
            yield return new WaitForSeconds(delay);

            // Stop physics movement
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
          

            agent.enabled = true;
            isPushBack = false;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!other.collider.CompareTag("NPCHitBox")) return;
            if (other.transform.CompareTag("NPCHitBox"))
            {
                Debug.Log($"{this.name} : Collision with NPCHitBox: {other.transform.name}");
                AManager otherManager = other.transform.GetComponentInParent<AManager>();
                bloodParticalSystem.Play();

                if (otherManager != null && otherManager != this)
                {
                    CrowdControlManager.Instance.TryRegister(this);
                    CrowdControlManager.Instance.TryRegister(otherManager);

                    isActiveInCombat = true;
                    otherManager.isActiveInCombat = true;

                    if (!enemyTargets.Contains(otherManager))
                    {
                        enemyTargets.Add(otherManager);
                        SortEnemyTargets();
                    }

                    Vector3 pushDir = movementDirection.sqrMagnitude > 0.001f ? movementDirection : transform.forward;
                    float pushForce = Random.Range(forceRange.x, forceRange.y);

                    ReceiveDamage(dealDamageAmount, otherManager);
                    HitAnimation();

                    ApplyPushback(-pushDir, pushForce, forceDuration);

                }
            }

            if (other.transform.CompareTag("Customer"))
            {
                Debug.Log($"{this.name} : Collision with NPC: {other.transform.name}");
                AManager otherManager = other.transform.GetComponentInParent<AManager>();

                if (otherManager != null && otherManager != this)
                {
                    CrowdControlManager.Instance.TryRegister(this);
                    CrowdControlManager.Instance.TryRegister(otherManager);

                    isActiveInCombat = true;
                    otherManager.isActiveInCombat = true;

                    if (!enemyTargets.Contains(otherManager))
                    {
                        enemyTargets.Add(otherManager);
                        SortEnemyTargets();
                    }

                    Vector3 pushDir = movementDirection.sqrMagnitude > 0.001f ? movementDirection : transform.forward;
                    float pushForce = Random.Range(forceRange.x, forceRange.y);

                    ApplyPushback(-pushDir, pushForce, forceDuration);
                }
            }
        }


        public void CombatStart()
        {
            foreach (var target in thisCollider)
            {
                //target.isTrigger = true;
            }
            foreach (var item in aController.weaponColliders)
            {
                //item.isTrigger = true;
            }
        }
    }
}
