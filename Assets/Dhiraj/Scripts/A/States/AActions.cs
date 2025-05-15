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



            /// wait timer settings
            timerDuration = Random.Range(timerDuration / 2, timerDuration * 2);

            timerRemaining = timerDuration;
            
            Debug.Log("Sitting");

            // Enable slider
            LeanTween.value(_aManager.gameObject, _aManager.sliderGroup.alpha, 1f, 01f)
                .setOnUpdate((float val) =>
                {
                    _aManager.sliderGroup.alpha = val;
                }).setOnComplete(()=>{
                    isTimerRunning = true;
                });
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
                _aManager.GoReturn = true;
                
                LeanTween.value(_aManager.gameObject, _aManager.sliderGroup.alpha, 0f, 0.2f)
               .setOnUpdate((float val) =>
               {
                   _aManager.sliderGroup.alpha = val;
               }).setOnComplete(() => {
                   _aManager.ChangeState(_aManager.aLocomotion);
               });
                // Optional: trigger event or callback here
            }




        }

        public override void EndState()
        {
            base.EndState();
            _aManager.agent.enabled = true;
            _aManager.transform.parent = null;
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
