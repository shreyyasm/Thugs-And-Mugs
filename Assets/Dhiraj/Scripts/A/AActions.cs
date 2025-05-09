using System.Linq;
using UnityEngine;

namespace Dhiraj
{
    public class AActions : ABase
    {
        public AActions(AManager aManager) : base(aManager)
        {
            _aManager = aManager;
        }

        private float timerDuration = 5;
        private float timerRemaining;
        private bool isTimerRunning = false;

        public override void StartState()
        {
            base.StartState();
            /// Sit and adust sitting position and rotation 
            _aManager.agent.enabled = false;
            _aManager.transform.parent = _aManager.seat.sitingPosition;
            _aManager.transform.localPosition = Vector3.zero;
            _aManager.transform.eulerAngles = Vector3.zero;
            ChangeAnimationState(CurrentState.Waiting);
            timerRemaining = timerDuration;
            isTimerRunning = true;
            Debug.Log("Sitting");
        }

        public override void UpdateState()
        {
            base.UpdateState();
            // Ask for Drink
            // 
            if (!isTimerRunning) return;

            timerRemaining -= Time.deltaTime;
            timerRemaining = Mathf.Clamp(timerRemaining, 0f, timerDuration);

            _aManager.fillImage.fillAmount = timerRemaining / timerDuration;

            if (timerRemaining <= 0f)
            {
                isTimerRunning = false;
                // Optional: trigger event or callback here
            }




        }

        public override void EndState()
        {
            base.EndState();
            _aManager.agent.enabled = true;
        }


        public void StartTimer(float duration)
        {
            timerDuration = duration;
            timerRemaining = duration;
            _aManager.fillImage.fillAmount = 1f;
            isTimerRunning = true;
        }
    }
}
