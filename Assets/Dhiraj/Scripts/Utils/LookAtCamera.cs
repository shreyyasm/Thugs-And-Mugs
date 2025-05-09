using UnityEngine;


namespace Dhiraj
{
    public class LookAtCamera : MonoBehaviour
    {
        private void Update()
        {
            //Will need to optimize
            LookTowardsCamera();
        }
        public void LookTowardsCamera()
        {
            if (Camera.main == null) return;

            Vector3 targetPosition = Camera.main.transform.position;

            // Keep the target's Y position same as this object
            targetPosition.y = transform.position.y;

            // Rotate to look at the camera on Y axis only
            transform.LookAt(targetPosition);
        }
    }
}
