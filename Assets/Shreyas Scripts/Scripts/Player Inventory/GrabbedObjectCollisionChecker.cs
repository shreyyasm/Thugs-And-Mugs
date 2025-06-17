using UnityEngine;

public class GrabbedObjectCollisionChecker : MonoBehaviour
{
    public bool isColliding = false;

    private int collisionCount = 0;

    private void OnCollisionEnter(Collision collision)
    {
        // Ignore collisions with the player or ignored layers if needed
        if(collision.gameObject.layer != 13)
        {
            collisionCount++;
            isColliding = true;
        }
      
    }

    private void OnCollisionExit(Collision collision)
    {
        collisionCount--;
        if (collisionCount <= 0)
        {
            collisionCount = 0;
            isColliding = false;
        }
    }

    // Optional: If using triggers instead of collisions
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != 13)
        {
            collisionCount++;
            isColliding = true;
        }
            
    }

    private void OnTriggerExit(Collider other)
    {
        collisionCount--;
        if (collisionCount <= 0)
        {
            collisionCount = 0;
            isColliding = false;
        }
    }
}
