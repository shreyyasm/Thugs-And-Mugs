using UnityEngine;


namespace Dhiraj
{
    public class GameManager : Singleton<GameManager>
    {
        public GameObject _PlayerObject;
        public SeatManager _SeatManager;    
        public bool isFightStarted = false;
        

        

        public void CutSceneStatus(bool isTrue)
        {

        }

        private void Start()
        {
            
        }
    }

}
