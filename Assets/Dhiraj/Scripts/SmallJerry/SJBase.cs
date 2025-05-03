using UnityEngine;

namespace Dhiraj
{
    public abstract class SJBase
    {
        public SJManager _sJManager;
        public SJBase(SJManager sJanager)
        {
            _sJManager = sJanager;
        }







        public virtual void StartState()
        {

        }
        public virtual void UpdateState()
        {

        }
        public virtual void ExitState()
        {

        }

    }
}