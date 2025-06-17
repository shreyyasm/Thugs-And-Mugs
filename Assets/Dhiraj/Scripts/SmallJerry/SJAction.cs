using Shreyas;
using UnityEngine;

namespace Dhiraj
{
    public class SJAction : SJBase
    {
        public SJAction(SJManager sJManager) : base(sJManager)
        {
            _sJManager = sJManager;
        }

        public override void StartState()
        {
            base.StartState();
            Debug.Log("Small Jerry Reached");
            ChangeAnimationState(CurrentState.Idle);
            /*LeanTween.delayedCall(0.5f, () => {
                _sJManager.ChangeState(_sJManager.Locomotion);
                _sJManager.GoReturn = true;
                //// Spwan Barrel at the current position in the some distance
            });*/
            ProcessNextItemWithDelay();
        }
        private int currentItemIndex = 0;
        private float delayBetweenItems = 0.7f; // adjust delay (in seconds)
        public override void UpdateState() {             
           
        }


        private void ProcessNextItemWithDelay()
        {
            if (currentItemIndex >= _sJManager.requestedItems.Count)
            {
                ChangeAnimationState(CurrentState.Normal);
                _sJManager.ChangeState(_sJManager.Locomotion);
                _sJManager.GoReturn = true;
                Debug.Log("All items processed.");
                return;
            }

            var currentItem = _sJManager.requestedItems[currentItemIndex];

            for (int j = 0; j < _sJManager.barrels.Count; j++)
            {
                if (currentItem.thisMenuData == _sJManager.barrels[j].menuItemData)
                {
                    _sJManager.InstantiateBarrel(_sJManager.barrels[j]);
                }
            }


            // Delay before processing the next item
            LeanTween.delayedCall(delayBetweenItems, () => {
                currentItemIndex++;
                ProcessNextItemWithDelay();
                });
        }

        public override void ExitState()
        {
            _sJManager.barrel.SetActive(false);
        }
    }
}
