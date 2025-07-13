using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    public GameObject bloodPrefab;
    public GameObject pos;
    void Start()
    {
       
    }
    public void DealDamage()
    {
       
      
      Instantiate(bloodPrefab, pos.transform.position + new Vector3(0, 0.4f, 0f), Quaternion.identity);
    }
}
