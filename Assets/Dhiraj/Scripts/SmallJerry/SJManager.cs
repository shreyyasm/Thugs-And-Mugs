using Shreyas;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Dhiraj
{
    public class SJManager : Singleton<SJManager>
    {
        #region State Initialize
        public SJBase currentState;
        public SJIdle Idle;
        public SJLocomotion Locomotion;
        public SJAction Action;
        #endregion 

        public string CurrentState;
        public WaypointBank waypointBank;
        public NavMeshAgent agent;
        public Animator anim;
        public GameObject barrel;
        public BarrelContainer barrelSpawnPoint;

        public List<CartItemData> requestedItems = new List<CartItemData>();
        public List<Barrel> barrels = new List<Barrel>();



        public bool isWalkingWithBarrel = false;
        public bool GoReturn = false;
        private void Start()
        {
            InitializeStates();
        }
        public void InitializeStates()
        {
            Idle = new SJIdle(this);
            Locomotion = new SJLocomotion(this);
            Action = new SJAction(this);
            currentState = Idle;
            currentState.StartState();
        }

        public void ChangeState(SJBase newState)
        {
            currentState.ExitState();
            currentState = newState;
            currentState.StartState();
            CurrentState = currentState.ToString();
        }

        private void Update()
        {
            currentState.UpdateState();
        }


        public Barrel InstantiateBarrel(Barrel br)
        {
            if (barrelSpawnPoint)
            {
                Barrel barrel = Instantiate(br);
                barrel.transform.parent = barrelSpawnPoint.transform;
                barrelSpawnPoint.ArrangeBarrale();
                return barrel;
            }
           return null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("BarrelContainer"))
            {
                barrelSpawnPoint = other.GetComponent<BarrelContainer>();
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("BarrelContainer"))
            {
                barrelSpawnPoint = null;
            }
        }


    }
}

