using UnityEngine;

public class HandsVisual : MonoBehaviour
{

    [SerializeField] Animator animator;

    [SerializeField] GameObject axe,broom,lighter; 
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if(Input.GetKey(KeyCode.Alpha1)){           //use Axe
            animator.SetBool("CanUseAxe",true);
            animator.SetBool("CanUseBroom",false);
            animator.SetBool("CanUseLighter",false);
            // axe.SetActive(true);
            // broom.SetActive(false);
            // lighter.SetActive(false);
        }
        else if(Input.GetKey(KeyCode.Alpha2)){     // use Broom
            animator.SetBool("CanUseAxe",false);
            animator.SetBool("CanUseBroom",true);
            animator.SetBool("CanUseLighter",false);
            // axe.SetActive(false);
            // broom.SetActive(true);
            // lighter.SetActive(false);
        }
        else if(Input.GetKey(KeyCode.Alpha3)){     // use Lighter
            animator.SetBool("CanUseAxe",false);
            animator.SetBool("CanUseBroom",false);
            animator.SetBool("CanUseLighter",true);
            // axe.SetActive(false);
            // broom.SetActive(false);
            // lighter.SetActive(true);
        }
        else if(Input.GetKey(KeyCode.Alpha4)){     // use Hands
            animator.SetBool("CanUseAxe",false);
            animator.SetBool("CanUseLighter",false);
            animator.SetBool("CanUseBroom",false);
            axe.SetActive(false);
            broom.SetActive(false);
            lighter.SetActive(false);
        }


        if(Input.GetKey(KeyCode.F)){
            if(animator.GetBool("CanUseAxe")){
                animator.SetBool("IsUsingAxe",true);
            }
            else if(animator.GetBool("CanUseBroom")){
                animator.SetBool("IsUsingBroom",true);
            }
            else{
                animator.SetBool("IsUsingAxe",false);
                animator.SetBool("IsUsingBroom",false);
                animator.SetBool("IsPunching",true);
            }   
        }else if( Input.GetKey(KeyCode.LeftShift)){
            animator.SetBool("IsBlocking",true);
        }else{
            animator.SetBool("IsUsingAxe",false);
            animator.SetBool("IsUsingBroom",false);
            animator.SetBool("IsPunching",false);
            animator.SetBool("IsBlocking",false);
        }



    }
}
