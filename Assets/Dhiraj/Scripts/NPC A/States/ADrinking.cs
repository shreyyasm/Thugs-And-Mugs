using UnityEngine;

namespace Dhiraj
{
    public class ADrinking : ABase
    {
        private float timerDuration;
        private float timerRemaining;
        private bool isTimerRunning = false;
        public ADrinking(AManager aManager) : base(aManager)
        {
            _aManager = aManager;
        }

        public override void StartState()
        {
            base.StartState();
            ChangeAnimationState(CurrentState.Drinking); // optional animation trigger
            timerDuration = Random.Range(7.5f, 30f); // 15/2 to 15*2
            timerRemaining = timerDuration;

            isTimerRunning = true;
            Debug.Log("Sitting started. Timer running.");

            _aManager.mug.SetActive(true);
        }
    

        public override void UpdateState()
        {
            base.UpdateState();
            UpdateTimerUI();
            if (timerRemaining <= 0f)
            {
                FinishWaiting();
                return;
            }

        }

        public override void EndState()
        {
            base.EndState();

            // Same cleanup as AActions.EndState(), for served NPCs
            _aManager.agent.enabled = true;
            _aManager.transform.parent = null;
            _aManager.seat.isOccupied = false;
            _aManager.seat.occupiedBy = null;

            if (_aManager.enemyTarget)
                _aManager.enemyTarget.GetComponent<AManager>().enemyTargets.Remove(_aManager);

            _aManager.sliderGroup.alpha = 0f;
            _aManager.mug.SetActive(false);
        }


        public void StartTimer(float duration)
        {
            timerDuration = duration;
            timerRemaining = duration;
            isTimerRunning = true;
            _aManager.fillImage.fillAmount = 1f;
        }
        private void FadeInSlider(System.Action onComplete = null)
        {
            LeanTween.value(_aManager.gameObject, _aManager.sliderGroup.alpha, 1f, 0.1f)
                .setOnUpdate(val => _aManager.sliderGroup.alpha = val)
                .setOnComplete(() => onComplete?.Invoke());
        }

        private void UpdateTimerUI()
        {
            timerRemaining -= Time.deltaTime;
            timerRemaining = Mathf.Clamp(timerRemaining, 0f, timerDuration);
            _aManager.fillImage.fillAmount = timerRemaining / timerDuration;
        }

        private void FinishWaiting()
        {
            isTimerRunning = false;
            _aManager.GoReturn = true;

            FadeOutSlider();
            _aManager.ChangeState(_aManager.aLocomotion);
        }

        private void FadeOutSlider()
        {
            LeanTween.value(_aManager.gameObject, _aManager.sliderGroup.alpha, 0f, 0.2f)
                .setOnUpdate(val => _aManager.sliderGroup.alpha = val);
        }


    }
}
