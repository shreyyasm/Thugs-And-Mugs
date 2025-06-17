using UnityEngine;

namespace Dhiraj
{
    public class NPCHitBox : MonoBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            /*if (collision.collider.CompareTag("NPC"))
            {
                Debug.Log("Collision with NPC detected: " + collision.collider.name);
                AManager self = GetComponentInParent<AManager>();
                AManager other = collision.collider.GetComponentInParent<AManager>();

                if (self != null && other != null && self != other)
                {
                    Vector3 pushDir = (other.transform.position - self.transform.position).normalized;
                    float force = Random.Range(self.forceRange.x, self.forceRange.y);

                    other.ApplyPushback(pushDir, force, self.forceDuration);
                    other.ReceiveDamage(self.dealDamageAmount, self);
                    other.HitAnimation();
                }
            }*/
        }
    }
}

