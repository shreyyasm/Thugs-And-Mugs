using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

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
        #endregion

        public string CurrentState;
        public WaypointBank waypointBank;
        public NavMeshAgent agent;
        public Animator anim;

        public GameObject mug;

        public bool GoReturn = false;
        public bool isLookAround = false;
        

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

            currentState = aIdle;
            currentState.StartState();
        }
        public void ChangeState(ABase newState)
        {
            currentState.EndState();
            currentState = newState;
            currentState.StartState();
        }
    }
}
