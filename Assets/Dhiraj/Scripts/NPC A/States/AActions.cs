using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Dhiraj
{
    public class AActions : ABase
    {
        public AActions(AManager aManager) : base(aManager) { }

        private float timerDuration;
        private float timerRemaining;
        private bool isTimerRunning = false;

        public override void StartState()
        {
            base.StartState();
            PrepareSitting();

            timerDuration = Random.Range(7.5f, 30f); // 15/2 to 15*2
            timerRemaining = timerDuration;

            FadeInSlider(() =>
            {
                isTimerRunning = true;
                Debug.Log("Sitting started. Timer running.");
            });
        }

        public override void UpdateState()
        {
            if (!isTimerRunning) return;

            if(!_aManager.playerInteracting)UpdateTimerUI();
            if(_aManager.isServed) _aManager.ChangeState(_aManager.aDrinking);

            if (timerRemaining <= 0f)
            {
                FinishWaiting();
                return;
            }

            CheckCombatTrigger();
        }

        public override void EndState()
        {
            base.EndState();
            if (timerRemaining <= 0f && !_aManager.isServed)
            {
                ResetSitting();
                FadeOutSlider();
            }
            
            if (_aManager.enemyTarget)
            {
                _aManager.enemyTarget.GetComponent<AManager>().enemyTargets.Remove(_aManager);
            }
        }

        public void StartTimer(float duration)
        {
            timerDuration = duration;
            timerRemaining = duration;
            isTimerRunning = true;
            _aManager.fillImage.fillAmount = 1f;
        }

        // ---------- Private Methods ---------- //

        private void PrepareSitting()
        {
            _aManager.agent.enabled = false;
            _aManager.transform.parent = _aManager.seat.sitingPosition;
            _aManager.transform.localPosition = Vector3.zero;
            _aManager.transform.eulerAngles = Vector3.zero;
            ChangeAnimationState(CurrentState.Waiting);
        }

        private void ResetSitting()
        {
            _aManager.agent.enabled = true;
            _aManager.transform.parent = null;
            _aManager.seat.isOccupied = false;
            _aManager.seat.occupiedBy = null;
        }

        private void FadeInSlider(System.Action onComplete = null)
        {
            LeanTween.value(_aManager.gameObject, _aManager.sliderGroup.alpha, 1f, 0.5f)
                .setOnUpdate(val => _aManager.sliderGroup.alpha = val)
                .setOnComplete(() => onComplete?.Invoke());
        }

        private void FadeOutSlider()
        {
            LeanTween.value(_aManager.gameObject, _aManager.sliderGroup.alpha, 0f, 0.2f)
                .setOnUpdate(val => _aManager.sliderGroup.alpha = val);
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

        private void CheckCombatTrigger()
        {
            if (_aManager.enemyTarget)
            {
                distanceFromTarget = Vector3.Distance(_aManager.transform.position, _aManager.enemyTarget.position);
                if (distanceFromTarget <= _aManager.agent.stoppingDistance)
                {
                    _aManager.ChangeState(_aManager.aCombatStance);
                }
            }
        }

        public void MarkAsServed()
        {
            _aManager.isServed = true;
            isTimerRunning = false; // stop timer
        }

        // 🔄 Placeholder for future actions (e.g., drinking)
        private void PerformDrinkAction()
        {
            // Hook drinking animation or event here
            Debug.Log($"{_aManager.name} is drinking...");
        }
    }
}
