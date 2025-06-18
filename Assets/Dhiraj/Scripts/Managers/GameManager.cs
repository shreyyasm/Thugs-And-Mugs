using Shreyas;
using System.Collections.Generic;
using UnityEngine;


namespace Dhiraj
{
    public class GameManager : Singleton<GameManager>
    {
        public int level;
        public GameObject _PlayerObject;
        public SeatManager _SeatManager;

        public InteractCanvas[] AllInteractCanvas;

        private FirstPersonMovementInputSystem FirstPersonMovementInputSystem;
        public bool isFightStarted;
        private void Awake()
        {
           AllInteractCanvas = Resources.FindObjectsOfTypeAll<InteractCanvas>();
            FirstPersonMovementInputSystem = FindAnyObjectByType<FirstPersonMovementInputSystem>();
            DisableMouseCursor();
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                foreach (InteractCanvas i in AllInteractCanvas)
                {
                    if (i != null && i.gameObject != null)
                    {
                        i.gameObject.SetActive(false);
                    }
                }
                FirstPersonMovementInputSystem.playerBusy = false;
                GameManager.Instance.DisableMouseCursor();
                FirstPersonMovementInputSystem.inventoryManager.SetInventoryCanvas(true);
            }
        }
        public void CutSceneStatus(bool isTrue)
        {

        }
        public void EnableMouseCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        public void DisableMouseCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

}
