using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using System;
using Dhiraj;
using EPOOutline;
using TreeEditor;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace Shreyas
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager instance;
        public bool isInteracting;
        [System.Serializable]
        public class InventoryItem
        {
            public ItemData data;
            public GameObject itemObject;
        }

        public Transform itemHolder;
        public Transform BarrelHolder;
        public GameObject[] handModels; // Match index to tag e.g., index 0 = "broom"
        public List<GameObject> BarrelsModels; // Match index to tag e.g., index 0 = "broom"
        public List<InventorySlot> uiSlots;

        private InventoryItem[] inventory = new InventoryItem[8];
        public int currentIndex = 0;
        public int totalItemInInventory = 0;

        public TextMeshProUGUI InteractSign;
        public FirstPersonMovementInputSystem firstPersonMovementInput;

        // Pickup variables
        public GameObject heldObject;
        private Rigidbody heldRB;

        [Header("Pickup Settings")]
        public float pickupMoveSpeed = 10f;
        public float hoverDistance = 2f;
        public float throwForce = 500f;

        private PlayerControls inputActions;
        private bool inputInteract;
        public bool inputDrop;
        private bool wasKeyJustReleased = false; // Detect if key was just released

        private bool inventoryEnabled = true;


        private void OnEnable() => inputActions.Enable();
        private void OnDisable() => inputActions.Disable();
        private void Awake()
        {
            instance = this;

            inputActions = new PlayerControls();
            inputActions.Player.Interact.performed += ctx => inputInteract = true;

            inputActions.Player.DropItem.performed += ctx => inputDrop = true;
            inputActions.Player.DropItem.canceled += ctx => inputDrop = false;
            inputActions.Player.Interact.canceled += ctx =>
            {
                inputInteract = false;
                wasKeyJustReleased = true; // Set flag when the key is released
            };

        }
        private void Start()
        {
            if (cameraSwayTransform != null)
            {
                defaultSwayPosition = cameraSwayTransform.localPosition;
                swayTargetPosition = defaultSwayPosition;

            }
            UpdateUI();
            UpdateHands();
        }



        RaycastHit hitGrab;
        public bool gunInHand;

        public float weaponUseInterval;
        public bool weaponUsing;

        void Update()
        {
            if (firstPersonMovementInput.playerBusy)
                return;

            HandleScroll();

            if (inputDrop)
            {
                DropCurrentItem();
                inputDrop = false; // Reset after use to prevent repeat calls
            }

            if (firstPersonMovementInput.isBlocking)
                return;

            UpdateHighlight();
            HandleInteractionRaycast();
            InteractByInventoryItems();
           

            if (isHoldingObject)
            {
                FollowCamera();
                HandleRotation();
                CheckCollisionAndSetMaterial();
                if (Input.GetMouseButtonDown(0)) // Place object
                {
                    PlaceObject();
                }
            }
            AcessBuildMenu();

            if(gunInHand)
            {
                if(Input.GetKeyDown(KeyCode.R) && !isReloading)
                {
                    
                    StartCoroutine(Reload(0.4f));
                }
            }
            if(weaponUseInterval > 0)
            {
                weaponUseInterval -= Time.deltaTime;
                weaponUsing = true;

            }
            else
            {
                weaponUsing = false;
            }
            if (cameraSwayTransform == null) return;

            // Smoothly move toward the target sway position
            cameraSwayTransform.localPosition = Vector3.SmoothDamp(
                cameraSwayTransform.localPosition,
                swayTargetPosition,
                ref swayVelocity,
                swaySmoothTime
            );

           
            float distanceSq = (cameraSwayTransform.localPosition - swayTargetPosition).sqrMagnitude;

            // Allow return when we're reasonably close (threshold scaled to sway amount)
            float returnThreshold = swayAmount * swayAmount * 0.5f; // e.g. 0.2 * 0.2 * 0.5 = 0.02

            if (distanceSq < returnThreshold && swayTargetPosition != defaultSwayPosition)
            {
                swayTargetPosition = defaultSwayPosition;
            }


        }
        private void FixedUpdate()
        {
            HandlePickup();
        }
        [SerializeField] private LayerMask interactLayerMask;
        [SerializeField] private float interactDistance = 3f;
        [SerializeField] private Camera playerCamera;

        private bool isSignVisible = false;
        private GameObject lastOutlinedObject = null;
        public GameObject VirtualCamera;
        public Animator ErrorAnim;
        public GameObject ErrorText;

        private void HandleInteractionRaycast()
        {
            if (!inventoryEnabled)
                return;



            Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);
            Ray ray = playerCamera.ScreenPointToRay(screenCenter);

            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayerMask))
            {
               
                if(hit.collider.gameObject.layer == 12)
                {
                    HandleRaycastHold(hit);
                   
                }

                GameObject hitObject = hit.collider.gameObject;
                // Cache components to avoid repeated GetComponent calls
                var interactable = hitObject.GetComponent<Interactable>();
                var pickup = hitObject.GetComponent<PickupTrigger>();
                var selectedItem = GetCurrentSelectedItem();

                if (interactable != null && interactable.CanBeInteracted)
                {
                    bool canInteract = string.IsNullOrEmpty(interactable.requiredItemTag) ||
                                       (selectedItem != null && selectedItem.itemTag == interactable.requiredItemTag);

                    if (canInteract)
                    {
                        ShowInteractSign("E - Interact");
                        isInteracting = true;
                        OutlineObject(hitObject);

                        if (Input.GetKeyDown(KeyCode.C))
                        {
                            if (hit.collider.GetComponent<Interactable>().interactableType == Interactable.InteractableType.CraftItem)
                            {
                               
                                if(tempBuildObject  != null)
                                    Destroy(tempBuildObject.gameObject);
                              
                            }
                        }
                        return;
                    }
                  
                }

                if (pickup != null)
                {
                    ShowInteractSign("E - Pick");
                    OutlineObject(hitObject);
                    isInteracting = true;

                    if (inputInteract)
                    {
                        if(totalItemInInventory < 8)
                        {
                            GameObject currentHand = handModels[currentIndex];
                            if (currentHand != null)
                                currentHand.SetActive(false);

                            DisableBarrels();

                            animator.SetBool("Interact", true);
                            SetAnimatorStates();

                            pickup.transform.SetParent(null);
                            pickup.transform.position = new Vector3(0, -0.3f, 1.5f);
                            PickupItem(pickup.itemData, pickup.gameObject);

                            LeanTween.delayedCall(0.4f, () => { UpdateHands(); });
                        }
                        else
                        {
                            //error
                            VirtualCamera.GetComponent<SimpleCameraLookInputSystem>().TriggerHeadNod();
                            ErrorText.SetActive(true);
                            ErrorText.GetComponent<TextMeshProUGUI>().text = "Inventory Full";
                            SFXManager.Instance.PlaySFX("Player/Can'tDo", 1);

                            LeanTween.delayedCall(1f, () => { ErrorAnim.SetTrigger("Fade"); });
                            LeanTween.delayedCall(1.5f, () => { ErrorText.SetActive(false); ErrorAnim.ResetTrigger("Fade"); });
                        }
                       
                    }
                   

                    return;
                }
            }
           
            // Shared cleanup
            isInteracting = false;
            HideInteractSign();
            ClearLastOutlined();
        }



        private void ShowInteractSign(string message)
        {
            //if (!isSignVisible)
            //{
            //    InteractSign.enabled = true;
            //    isSignVisible = true;
            //}

            //InteractSign.text = message;
        }

        public void HideInteractSign()
        {
            if (isSignVisible)
            {
                InteractSign.enabled = false;
                isSignVisible = false;
            }
        }

        private void OutlineObject(GameObject obj)
        {
            if (obj != lastOutlinedObject)
            {
                DisableOutline(lastOutlinedObject);
                
                EnableOutline(obj);
                lastOutlinedObject = obj;
            }
        }

        public void ClearLastOutlined()
        {
            DisableOutline(lastOutlinedObject);
            lastOutlinedObject = null;
        }


        private bool axeAnimToggle = false;
        public GameObject puffparticle;
        private int katanaAnimIndex = -1; // Start at -1 so first becomes 0
        public bool usingWeaponDuration;

        public ParticleSystem shine;
        public void InteractByInventoryItems()
        {
          
            if (Input.GetMouseButtonDown(0) && GetCurrentSelectedItem() != null && GetCurrentSelectedItem().MouseUse)
            {
                ItemData selectedItem = GetCurrentSelectedItem();
                if (selectedItem != null && inventoryEnabled)
                {
                    string tag = selectedItem.itemTag;

                    switch (tag)
                    {
                        case "Broom":
                            if (animator.GetBool("CanUseBroom"))
                                animator.SetTrigger("IsUsing");

                            SetAnimatorStates("CanUseBroom");


                            break;

                        case "Axe":
                            if (animator.GetBool("CanUseAxe"))
                            {
                                if (!weaponUsing)
                                {
                                    // Alternate between 0 and 1 for the sub animation
                                    int index = axeAnimToggle ? 1 : 0;
                                    animator.SetInteger("AxeAnimIndex", index);

                                    // Trigger a shared "IsUsing" animation
                                    animator.SetTrigger("IsUsing");

                                    // Flip toggle
                                    axeAnimToggle = !axeAnimToggle;

                                    TrailRenderer trail = handModels[0].GetComponentInChildren<TrailRenderer>();
                                    if (index == 0)
                                    {
                                        LeanTween.delayedCall(0.14f, () => { SFXManager.Instance.PlaySFX("Inventory/Slash", 0.7f, 0.6f); });
                                        LeanTween.delayedCall(0.12f, () => { if (trail != null) trail.enabled = true; usingWeaponDuration = true; });
                                        LeanTween.delayedCall(0.1f, () => { TriggerMeleeSway(false); });

                                    }

                                    else if (index == 1)
                                    {
                                        LeanTween.delayedCall(0.14f, () => { SFXManager.Instance.PlaySFX("Inventory/Slash", 0.7f, -0.6f); });
                                        LeanTween.delayedCall(0.12f, () => { if (trail != null) trail.enabled = true; usingWeaponDuration = true; });
                                        LeanTween.delayedCall(0.1f, () => { TriggerMeleeSway(true ); }); 

                                    }

                                    weaponUseInterval += 0.8f;
                                   

                                    LeanTween.delayedCall(0.3f, () => { if (trail != null) trail.enabled = false; });

                                    LeanTween.delayedCall(0.5f, () => { usingWeaponDuration = false; });

                                    SetAnimatorStates("CanUseAxe");
                                }
                                    
                            }
                           
                            break;

                        case "Lighter":
                             SetAnimatorStates("CanUseLighter");
                            break;

                        case "Hammer":
                            if (animator.GetBool("CanUseHammer"))
                            {
                                if (!weaponUsing)
                                {
                                    animator.SetTrigger("IsUsing");

                                    //TrailRenderer trail = handModels[14].GetComponentInChildren<TrailRenderer>();
                                   
                                    //LeanTween.delayedCall(0.14f, () => { SFXManager.Instance.PlaySFX("Inventory/Slash", 0.7f, 0.6f); });
                                    //LeanTween.delayedCall(0.14f, () => { if (trail != null) trail.enabled = true; });


                                    //weaponUseInterval += 0.7f;
                                    //usingWeaponDuration = true;

                                    //LeanTween.delayedCall(0.3f, () => { if (trail != null) trail.enabled = false; });

                                    //LeanTween.delayedCall(0.5f, () => { usingWeaponDuration = false; });


                                    SetAnimatorStates("CanUseHammer");
                                }
                            }
                                
                                   
                            break;

                        case "Katana":
                            if (animator.GetBool("CanUseKatana"))
                            {
                              
                                if(!weaponUsing)
                                {
                                    // Cycle through 0 → 1 → 2 → 0 ...
                                    katanaAnimIndex = (katanaAnimIndex + 1) % 3;
                                    animator.SetInteger("KatanaAnimIndex", katanaAnimIndex);

                                    // Trigger the animation
                                    animator.SetTrigger("IsUsing");
                                   
                                    TrailRenderer trail = handModels[20].GetComponentInChildren<TrailRenderer>();
                                    if (katanaAnimIndex == 0)
                                    {
                                        LeanTween.delayedCall(0.14f, () => { SFXManager.Instance.PlaySFX("Inventory/Slash", 0.7f, 0.6f);});
                                        LeanTween.delayedCall(0.1f, () => { if(trail != null) trail.enabled = true; usingWeaponDuration = true; });
                                        LeanTween.delayedCall(0.16f, () => { TriggerMeleeSway(false); });
                                    }

                                    else if (katanaAnimIndex == 1)
                                    {
                                        LeanTween.delayedCall(0.14f, () => { SFXManager.Instance.PlaySFX("Inventory/Slash", 0.7f, -0.6f); });
                                        LeanTween.delayedCall(0.1f, () => { if (trail != null) trail.enabled = true; usingWeaponDuration = true;});
                                        LeanTween.delayedCall(0.14f, () => { TriggerMeleeSway(true); });
                                    }


                                    else if (katanaAnimIndex == 2)
                                    {
                                        LeanTween.delayedCall(0.14f, () => { SFXManager.Instance.PlaySFX("Inventory/Slash", 0.7f, 0f); usingWeaponDuration = true; TriggerMeleeSway(false,true); });
                                    }

                                    weaponUseInterval += 1f;
                                   

                                    LeanTween.delayedCall(0.35f, () => { if (trail != null) trail.enabled = false; });

                                    LeanTween.delayedCall(0.5f, () => { usingWeaponDuration = false; });
                                    SetAnimatorStates("CanUseKatana");
                                }
                               
                              
                            }
                            
                            break;

                        case "Knucles":
                            if (animator.GetBool("CanUseKnucles"))
                            {
                                if (!weaponUsing)
                                {
                                    // Alternate between 0 and 1 for the sub animation
                                    int index = axeAnimToggle ? 1 : 0;
                                    animator.SetInteger("KnuclesAnimIndex", index);

                                    // Trigger a shared "IsUsing" animation
                                    animator.SetTrigger("IsUsing");

                                    // Flip toggle
                                    axeAnimToggle = !axeAnimToggle;
                                    TrailRenderer trail = null;

                                  
                                    if (index == 0)
                                    {
                                        trail = handModels[22].GetComponentInChildren<TrailRenderer>();
                                       SFXManager.Instance.PlaySFX($"Player/PunchThrow", 0.6f, -0.5f);
                                        LeanTween.delayedCall(0.1f, () => { TriggerMeleeSway(false); });
                                        if (trail != null) trail.enabled = true; usingWeaponDuration = true;

                                    }

                                    else if (index == 1)
                                    {
                                        trail = handModels[15].GetComponentInChildren<TrailRenderer>();
                                       SFXManager.Instance.PlaySFX($"Player/PunchThrow", 0.6f, 0.5f);
                                        LeanTween.delayedCall(0.1f, () => { TriggerMeleeSway(true); });
                                        if (trail != null) trail.enabled = true; usingWeaponDuration = true;

                                    }

                                    weaponUseInterval += 0.8f;
                                   

                                    LeanTween.delayedCall(0.2f, () => { if (trail != null) trail.enabled = false; });

                                    LeanTween.delayedCall(0.03f, () => { usingWeaponDuration = false; });
                                    SetAnimatorStates("CanUseKnucles");
                                   
                                }
                                    
                            }
                            break;

                        case "Sickle":
                            if (animator.GetBool("CanUseSickle"))
                            {
                                if (!weaponUsing)
                                {
                                    // Alternate between 0 and 1 for the sub animation
                                    int index = axeAnimToggle ? 1 : 0;
                                    animator.SetInteger("SickleAnimIndex", index);

                                    // Trigger a shared "IsUsing" animation
                                    animator.SetTrigger("IsUsing");

                                    // Flip toggle
                                    axeAnimToggle = !axeAnimToggle;

                                    TrailRenderer trail = handModels[21].GetComponentInChildren<TrailRenderer>();
                                    if (index == 0)
                                    {
                                        LeanTween.delayedCall(0.14f, () => { SFXManager.Instance.PlaySFX("Inventory/Slash", 0.7f, 0.6f); });
                                        LeanTween.delayedCall(0.1f, () => { if (trail != null) trail.enabled = true; });
                                        LeanTween.delayedCall(0.16f, () => { TriggerMeleeSway(true); });
                                    }

                                    else if (index == 1)
                                    {
                                        LeanTween.delayedCall(0.14f, () => { SFXManager.Instance.PlaySFX("Inventory/Slash", 0.7f, -0.6f); });
                                        LeanTween.delayedCall(0.1f, () => { if (trail != null) trail.enabled = true; });
                                        LeanTween.delayedCall(0.16f, () => { TriggerMeleeSway(true); });
                                    }

                                    weaponUseInterval += 0.8f;
                                    usingWeaponDuration = true;

                                    LeanTween.delayedCall(0.4f, () => { if (trail != null) trail.enabled = false; });

                                    LeanTween.delayedCall(0.5f, () => { usingWeaponDuration = false; });
                                    SetAnimatorStates("CanUseSickle");
                                }
                                    
                            }
                           
                            break;

                        case "Gun":
                           
                            SetAnimatorStates("CanUseGun");
                            if (currentAmmo > 0 && !isReloading && Time.time >= nextTimeToFire && currentAmmo > 0)
                            {
                                nextTimeToFire = Time.time + 0.4f;
                                if (animator.GetBool("CanUseGun"))
                                    animator.SetTrigger("IsUsing");
                                Shoot();
                            }

                          
                            break;

                        case "Shortgun":
                           

                            if (currentAmmo > 0 && !isReloading && Time.time >= nextTimeToFire && currentAmmo > 0)
                            {
                                nextTimeToFire = Time.time + 0.6f;
                                if (animator.GetBool("CanUseShortgun"))
                                    animator.SetTrigger("IsUsing");
                                Shoot();
                            }
                        
                            SetAnimatorStates("CanUseShortgun");
                            break;

                        case "Bat":
                            if (animator.GetBool("CanUseBat"))
                            {
                                if (!weaponUsing)
                                {
                                    animator.SetTrigger("IsUsing");
                                    SetAnimatorStates("CanUseBat");


                                    TrailRenderer trail = handModels[24].GetComponentInChildren<TrailRenderer>();

                                    LeanTween.delayedCall(0.16f, () => { SFXManager.Instance.PlaySFX("Inventory/Slash", 0.7f, 0.6f); });
                                    LeanTween.delayedCall(0.2f, () => { if (trail != null) trail.enabled = true; usingWeaponDuration = true;
                                    });
                                    LeanTween.delayedCall(0.15f, () => { TriggerMeleeSway(true); });
                                    weaponUseInterval += 0.9f;
                                   
                                    LeanTween.delayedCall(0.38f, () => { if (trail != null) trail.enabled = false; });

                                    LeanTween.delayedCall(0.5f, () => { usingWeaponDuration = false; });
                                }

                            }

                             
                            break;

                        case "Knife":
                            if (animator.GetBool("CanUseKnife"))
                            {
                                if (!weaponUsing)
                                {
                                    // Alternate between 0 and 1 for the sub animation
                                    int index = axeAnimToggle ? 1 : 0;
                                    animator.SetInteger("KnifeAnimIndex", index);

                                    // Trigger a shared "IsUsing" animation
                                    animator.SetTrigger("IsUsing");

                                    // Flip toggle
                                    axeAnimToggle = !axeAnimToggle;

                                    TrailRenderer trail = handModels[25].GetComponentInChildren<TrailRenderer>();
                                    if (index == 0)
                                    {
                                        LeanTween.delayedCall(0.14f, () => { SFXManager.Instance.PlaySFX("Inventory/Slash", 0.7f, 0.6f); });
                                        LeanTween.delayedCall(0.1f, () => { TriggerMeleeSway(true); });
                                        LeanTween.delayedCall(0.1f, () => { if (trail != null) trail.enabled = true; usingWeaponDuration = true; });

                                    }

                                    else if (index == 1)
                                    {
                                        LeanTween.delayedCall(0.14f, () => { SFXManager.Instance.PlaySFX("Inventory/Slash", 0.7f, -0.6f); });
                                        LeanTween.delayedCall(0.1f, () => { TriggerMeleeSway(false); });
                                        if (trail != null) 
                                            trail.enabled = true; 
                                        usingWeaponDuration = true;

                                    }

                                    weaponUseInterval += 0.7f;
                                    

                                    LeanTween.delayedCall(0.28f, () => { if (trail != null) trail.enabled = false; });

                                    LeanTween.delayedCall(0.5f, () => { usingWeaponDuration = false; });
                                    SetAnimatorStates("CanUseKnife");
                                }
                                    
                            }
                          
                            break;


                        default:
                            //animator.SetBool("IsUsing", false);
                            break;
                    }
                }
                else
                {
                    //animator.SetBool("IsUsing", false);
                }

                if (wasKeyJustReleased)
                    wasKeyJustReleased = false;

                Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);
                Ray ray = playerCamera.ScreenPointToRay(screenCenter);
                if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayerMask) && isInteracting)
                {
                    InventoryItem currentItem = inventory[currentIndex];
                    GameObject hitObj = hit.collider.gameObject;
                    Interactable interactable = hitObj.GetComponent<Interactable>();

                    if (interactable != null && interactable.CanBeInteracted)
                    {
                        if (interactable.interactableType == Interactable.InteractableType.TreeInteraction)
                        {
                            LeanTween.delayedCall(0.25f, () => interactable.Interact(selectedItem));
                        }
                        
                        else if (interactable.interactableType == Interactable.InteractableType.CraftItem)
                        {
                            if (PlayerStatics.Instance.Woods > 0)
                            {
                                if (inventory[currentIndex].data.itemTag == "Hammer")
                                {
                                    LeanTween.delayedCall(0.25f, () =>
                                    {
                                        interactable.Interact(selectedItem);
                                        Instantiate(puffparticle, hit.point, Quaternion.identity);
                                    });
                                }
                            }
                            else
                            {
                                //Show No woods and SFX
                            }



                        }
                    }
                }
            }
            if (inputInteract)
            {
                ItemData selectedItem = GetCurrentSelectedItem();
               
                if (wasKeyJustReleased)
                    wasKeyJustReleased = false;

                Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);
                Ray ray = playerCamera.ScreenPointToRay(screenCenter);
                if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayerMask) && isInteracting)
                {
                    InventoryItem currentItem = inventory[currentIndex];
                    GameObject hitObj = hit.collider.gameObject;
                    Interactable interactable = hitObj.GetComponent<Interactable>();

                    if (interactable != null && interactable.CanBeInteracted && interactable.E_Interaction)
                    {
                       
                        interactable.Interact(selectedItem);


                        if (heldObject == null && interactable.isPickable)
                        {
                            PickUpObject(hitObj);
                            HideInteractSign();
                            ClearLastOutlined();

                            SetInventoryEnabled(false);
                            animator.SetBool("InterectHold", true);
                            animator.SetBool("IsBlocking", false);
                        }
                        if(interactable.interactableType == Interactable.InteractableType.BarrelInteraction && currentItem.itemObject.GetComponent<Barrel>())
                        {
                           
                            interactable.Interact(selectedItem, currentItem.itemObject);

                            Barrel currentBarrel = currentItem.itemObject.GetComponent<Barrel>();
                            string currentBarrelName = currentBarrel.Name;

                            for (int i = 0; i < BarrelsModels.Count; i++)
                            {
                                GameObject barrelObj = BarrelsModels[i];
                                Animator barrelAnimator = barrelObj.GetComponent<Animator>();
                                Barrel barrelData = barrelObj.GetComponent<Barrel>();

                                barrelAnimator.SetBool("OpenBarrel", barrelData.Name == currentBarrelName);
                            }
                        }
                    }
                }
            }

        }

        [SerializeField] private Transform cameraSwayTransform;
        [SerializeField] private float swayAmount = 0.1f;
        [SerializeField] private float swayY = 1f;
        [SerializeField] private float swaySmoothTime = 0.1f;

        private Vector3 defaultSwayPosition;
        private Vector3 swayTargetPosition;
        private Vector3 swayVelocity;

        public void TriggerMeleeSway(bool isLeftStrike, bool middle = false)
        {
            if (cameraSwayTransform == null) return;

            float direction = isLeftStrike ? -1f : 1f;
            if(!middle)
            {
                swayTargetPosition = defaultSwayPosition + new Vector3(
              direction * swayAmount,
              swayAmount * swayY, // vertical sway
              0f);
            }
            else
            {
                swayTargetPosition = defaultSwayPosition + new Vector3(
            0,
            swayAmount * swayY, // vertical sway
            0f);
            }
          
        }


        private void SetAnimatorStates(string activeState = null)
        {
            string[] allStates = new string[]
            {
            "CanUseAxe",
            "CanUseBroom",
            "CanUseLighter",
            "UseDrink",
            "UseBarrel",
            "CanUseHammer",
            "CanUseGun",
            "CanUseShortgun",
            "CanUseKatana",
            "CanUseSickle",
            "CanUseKnucles",
            "CanUseBat",
            "CanUseKnife"
            };

            foreach (string state in allStates)
            {
                bool shouldBeActive = !string.IsNullOrEmpty(activeState) && state == activeState;
                animator.SetBool(state, shouldBeActive);
            }
            //animator.ResetTrigger("IsUsing");
           
        }



        public void SetDrinkBlend(float value)
        {
            animator.ResetTrigger("IsUsing");
            animator.SetFloat("Drinks", value);
        }

        private void EnableOutline(GameObject obj)
        {
            if (obj == null) return;
            if (obj.CompareTag("Customer") && obj != null)
            {
                Outlinable outline = obj.transform.root.gameObject.GetComponent<Outlinable>();
                if (outline != null)
                {
                    outline.enabled = true;
                }
            }
            else
            {
                Outlinable outline = obj.GetComponent<Outlinable>();
                if (outline != null)
                {
                    outline.enabled = true;
                }
            }
            
        }

        private void DisableOutline(GameObject obj)
        {
            if (obj == null) return;

            Outlinable outline = obj.transform.root.gameObject.GetComponent<Outlinable>();
            Outlinable outlineIn = obj.GetComponent<Outlinable>();
            if (outline != null)
            {
                outline.enabled = false;
            }
            if (outlineIn != null)
            {
               
                outlineIn.enabled = false;
            }
        }


        public ItemData GetCurrentSelectedItem()
        {
            if (inventory[currentIndex] != null)
                return inventory[currentIndex].data;
            return null;
        }

        public void PickupItem(ItemData itemData, GameObject itemInWorld)
        {
            int index = GetFirstEmptySlot();
            if (index == -1) return;

            GameObject storedItem = itemInWorld;
            storedItem.GetComponent<Rigidbody>().isKinematic = true;
            storedItem.GetComponent<Rigidbody>().useGravity = false;

            if (storedItem.CompareTag("Barrel"))
            {

                foreach (Transform i in storedItem.GetComponentsInChildren<Transform>())
                {
                    i.gameObject.layer = 11;
                }
                   
                storedItem.transform.SetParent(BarrelHolder);
                storedItem.transform.localPosition = Vector3.zero;
                storedItem.transform.localRotation = Quaternion.identity;
                storedItem.SetActive(false);
                storedItem.GetComponent<Outlinable>().enabled = false;
                storedItem.GetComponent<BoxCollider>().enabled = false;
                BarrelsModels.Add(storedItem);
                
            }
            else if (storedItem.CompareTag("Mug"))
            {
                if (storedItem.GetComponent<Mug>().isFilled)
                {
                    handModels[6].GetComponent<MugInHand>().LiquidShaderController.baseColor = storedItem.GetComponent<Mug>().liquidShaderController.baseColor;
                    handModels[6].GetComponent<MugInHand>().LiquidShaderController.emissionColor = storedItem.GetComponent<Mug>().liquidShaderController.baseColor;
                    handModels[6].GetComponent<MugInHand>().LiquidShaderController.fillAmount = storedItem.GetComponent<Mug>().liquidShaderController.fillAmount;
                }

            }
            else
            {
                storedItem.transform.SetParent(itemHolder);
                storedItem.SetActive(false);
            }


            inventory[index] = new InventoryItem { data = itemData, itemObject = storedItem };
            //Destroy(itemInWorld);
            totalItemInInventory++;
            SFXManager.Instance.PlaySFX("Player/Pickup", 0.2f);
           
            UpdateUI();

        }
        public void PickupBarrel()
        {
            InventoryItem currentItem = inventory[currentIndex];
            if (currentItem != null && currentItem.itemObject != null)
            {
                Barrel barrelComponent = currentItem.itemObject.GetComponent<Barrel>();


                if (barrelComponent != null)
                {
                    string targetTag = barrelComponent.itemType.ToString();
                    for (int i = 0; i < BarrelsModels.Count; i++)
                    {
                        BarrelsModels[i].SetActive(false);
                        string var = BarrelsModels[i].GetComponent<Barrel>().Name;
                        string var2 = barrelComponent.Name;
                        if (var == var2)
                        {

                            BarrelsModels[i].SetActive(true);
                        }
                    }
                }

            }
        }
        private int GetFirstEmptySlot()
        {
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i] == null) return i;
            }
            return -1;
        }
        private void DropCurrentItem()
        {
            InventoryItem currentItem = inventory[currentIndex];
            if (currentItem == null || currentItem.itemObject == null)
                return;

            GameObject droppedItem = currentItem.itemObject;

            if (droppedItem.CompareTag("Mug"))
            {
                if (droppedItem.GetComponent<Mug>().isFilled)
                {
                    handModels[6].GetComponent<MugInHand>().LiquidShaderController.baseColor = Color.white;
                    handModels[6].GetComponent<MugInHand>().LiquidShaderController.emissionColor = Color.white;
                    handModels[6].GetComponent<MugInHand>().LiquidShaderController.fillAmount = 0;
                }

            }
            if (droppedItem.CompareTag("Barrel"))
            {
                foreach (Transform i in droppedItem.GetComponentsInChildren<Transform>())
                {
                    i.gameObject.layer = 6;
                }
            }
              
            // Cache world position/rotation/scale
            Vector3 worldPos = droppedItem.transform.position;
            Quaternion worldRot = droppedItem.transform.rotation;
            Vector3 worldScale = droppedItem.transform.lossyScale;

            // Unparent and restore transform manually
            droppedItem.transform.SetParent(null);
            droppedItem.transform.position = worldPos;
            droppedItem.transform.rotation = worldRot;
            droppedItem.transform.localScale = worldScale; // Apply world scale directly

            // Position it in front of the player
            droppedItem.transform.position = itemHolder.transform.position + playerCamera.transform.forward;
            droppedItem.transform.rotation = Quaternion.identity;
            droppedItem.GetComponent<BoxCollider>().enabled = true;
            droppedItem.SetActive(true);

            // Enable physics
            Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.AddForce(playerCamera.transform.forward * 2f, ForceMode.Impulse);

            }

            Collider col = droppedItem.GetComponent<Collider>();
            if (col != null)
                col.enabled = true;

            // Clear the slot
            inventory[currentIndex] = null;
            if(totalItemInInventory > 0) 
                totalItemInInventory--;
            UpdateUI();
            UpdateHands();
            UpdateHighlight();
            BarrelsModels.Remove(droppedItem);
            Debug.Log("Drop");
        }
        public GameObject tempGameObject;

        public void SetTemp_GameObject(int index)
        {
            InventoryItem currentItem = inventory[index];
            if (currentItem == null || currentItem.itemObject == null)
                return;

            GameObject droppedItem = currentItem.itemObject;
            tempGameObject = droppedItem;
        }
        public void DropItemByChoice(int index, Transform parent)
        {
            InventoryItem currentItem = inventory[index];
            if (currentItem == null || currentItem.itemObject == null)
                return;

            GameObject droppedItem = currentItem.itemObject;
            tempGameObject = droppedItem;

            // Cache world scale before reparenting
            Vector3 worldScale = droppedItem.transform.lossyScale;

            // Set parent and maintain world position/rotation/scale
            droppedItem.transform.SetParent(parent, true);

            // Force exact local alignment
            droppedItem.transform.localPosition = Vector3.zero;
            droppedItem.transform.localRotation = Quaternion.Euler(-90, 0, 0);

            // Correct scale relative to new parent
            Vector3 parentScale = parent.lossyScale;
            droppedItem.transform.localScale = new Vector3(
                worldScale.x / parentScale.x,
                worldScale.y / parentScale.y,
                worldScale.z / parentScale.z
            );

            // Enable colliders and object
            if (droppedItem.TryGetComponent(out Collider col))
                col.enabled = true;

            droppedItem.SetActive(true);

            // Clear inventory slot
            inventory[index] = null;
            UpdateUI();
            UpdateHands();
            UpdateHighlight();
            droppedItem.layer = 0;
            BarrelsModels.Remove(droppedItem);

        }


        public void ClearInventorySlot()
        {
            // Clear the slot
            inventory[currentIndex] = null;
            UpdateUI();
            UpdateHands();
            UpdateHighlight();
        }

        private void HandleScroll()
        {
            if (!inventoryEnabled) return;

            int previousIndex = currentIndex;

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0f)
            {
                currentIndex = (currentIndex + 1) % inventory.Length;
            }
            else if (scroll < 0f)
            {
                currentIndex = (currentIndex - 1 + inventory.Length) % inventory.Length;
            }

            if (currentIndex != previousIndex)
            {
                UpdateHands();  // Only call when index actually changes
                SFXManager.Instance.PlaySFX("Inventory/InventorySelect", 0.1f);
            }

        }
        [SerializeField] Animator animator;
        public bool holdingSomething;
        public TextMeshProUGUI selectedItemName;
        public void UpdateHands()
        {
            if (!inventoryEnabled) return;

            // Always start by disabling all hand models
            for (int i = 0; i < handModels.Length; i++)
            {
                if (handModels[i] != null)
                    handModels[i].SetActive(false);
                gunInHand = false;
              
            }

            // If there's no item in the selected slot, skip everything else
            if (inventory[currentIndex]?.data == null)
            {
                SetAnimatorStates();
                holdingSomething = false;
                selectedItemName.text =  null;
                return;
            }
            else
                holdingSomething = true;

            string tag = inventory[currentIndex].data.itemTag;
            selectedItemName.text = inventory[currentIndex].data.itemName;
            for (int i = 0; i < handModels.Length; i++)
            {
               

                if (handModels[i] != null)
                {
                    if (handModels[i].CompareTag(tag))
                    {
                       
                        handModels[i].SetActive(true);

                        switch (tag)
                        {
                            
                            case ("Barrel"):

                                if (animator.GetBool("UseBarrel"))
                                {
                                    animator.SetBool("UseBarrel", false);
                                    LeanTween.delayedCall(0.1f, () => { animator.SetBool("UseBarrel", true); });
                                }
                                else
                                    SetAnimatorStates("UseBarrel");
                                
                                break;

                            case "Broom":
                                SetAnimatorStates("CanUseBroom");
                                
                                break;

                            case "Axe":                              
                                SetAnimatorStates("CanUseAxe");
                               
                                break;

                            case ("Lighter"):        
                                SetAnimatorStates("CanUseLighter");
                                LeanTween.delayedCall(0.05f, () => { SFXManager.Instance.PlaySFX("Inventory/LighterOpen", 0.6f,0.45f); });
                             
                               
                                break;

                            case ("BlackBlaze"):
                                SetAnimatorStates("UseDrink");
                              
                                break;

                            case ("CactusBomb"):
                                SetAnimatorStates("UseDrink");
                                SetDrinkBlend(2f);
                                break;

                            case ("DesertDraught"):
                                SetAnimatorStates("UseDrink");
                                SetDrinkBlend(3f);
                                break;

                            case ("FireOut"):
                                SetAnimatorStates("UseDrink");
                                SetDrinkBlend(4f);
                                break;

                            case ("FrothyMug"):
                                SetAnimatorStates("UseDrink");
                                SetDrinkBlend(5f);
                                break;

                            case ("ShinerSip"):
                                SetAnimatorStates("UseDrink");
                                SetDrinkBlend(6f);
                                break;

                            case ("SnakeBite"):
                                SetAnimatorStates("UseDrink");
                                SetDrinkBlend(7f);
                                break;

                            case ("StraightShot"):
                                SetAnimatorStates("UseDrink");
                                SetDrinkBlend(8f);
                                break;
                                 
                            case ("WidowKiss"):
                                SetAnimatorStates("UseDrink");
                                SetDrinkBlend(9f);
                                break;

                            case ("Mug"):
                                SetAnimatorStates("UseDrink"); 
                                SetDrinkBlend(10f);

                                InventoryItem currentItem = inventory[currentIndex];
                                if (currentItem == null || currentItem.itemObject == null)
                                    return;

                                GameObject droppedItem = currentItem.itemObject;

                                if (droppedItem.GetComponent<Mug>().isFilled)
                                {
                                    handModels[6].GetComponent<MugInHand>().LiquidShaderController.baseColor = droppedItem.GetComponent<Mug>().liquidShaderController.baseColor;
                                    handModels[6].GetComponent<MugInHand>().LiquidShaderController.emissionColor = droppedItem.GetComponent<Mug>().liquidShaderController.baseColor;
                                    handModels[6].GetComponent<MugInHand>().LiquidShaderController.fillAmount = droppedItem.GetComponent<Mug>().liquidShaderController.fillAmount;
                                }
                                else
                                {
                                    handModels[6].GetComponent<MugInHand>().LiquidShaderController.fillAmount = 0;
                                }


                                break;

                            case ("Hammer"):                              
                                SetAnimatorStates("CanUseHammer");
                               
                                break;

                            case "Katana":                               
                                SetAnimatorStates("CanUseKatana");
                                LeanTween.delayedCall(0.05f, () => { SFXManager.Instance.PlaySFX("Inventory/KatanaOut", 0.2f); });
                                LeanTween.delayedCall(0.1f, () => { shine.Play(); });


                                break;

                            case "Knucles":                               
                                SetAnimatorStates("CanUseKnucles");
                               
                                break;

                            case "Sickle":                              
                                SetAnimatorStates("CanUseSickle");
                                
                                break;

                            case "Gun":                              
                                gunInHand = true;
                                SetAnimatorStates("CanUseGun");
                                currentGun = GunType.Revolver;
                                maxAmmo = inventory[currentIndex].data.TotalBullets;
                                currentAmmo = inventory[currentIndex].data.CurrentBullets;
                                FirePoint = firePointGun;
                                LeanTween.delayedCall(0.05f, () => { SFXManager.Instance.PlaySFX("Firearm/GunOut", 0.35f); });
                                originalCamPosition = playerCamera.transform.localPosition;
                                UpdateAmmoUI();
                                if (currentAmmo <= 0 && !isReloading)
                                {
                                    StartCoroutine(Reload(0.9f));
                                }

                                break;

                            case "Shortgun":                             
                                SetAnimatorStates("CanUseShortgun");
                                gunInHand = true;
                                currentGun = GunType.Shotgun;
                                maxAmmo = inventory[currentIndex].data.TotalBullets;
                                currentAmmo = inventory[currentIndex].data.CurrentBullets;
                                FirePoint = firePointShortGun;

                                originalCamPosition = playerCamera.transform.localPosition;
                                UpdateAmmoUI();
                                if (currentAmmo <= 0 && !isReloading)
                                {
                                    StartCoroutine(Reload(0.9f));
                                }
                                break;

                            case "Bat":
                                SetAnimatorStates("CanUseBat");
                               
                                break;

                            case "Knife":
                                SetAnimatorStates("CanUseKnife");
                               
                                break;

                            // Add more cases for new types
                            default:
                               
                                break;
                        }
                    }
                }

            }
        }


        private void UpdateUI()
        {
            for (int i = 0; i < uiSlots.Count; i++)
            {
                if (inventory[i] != null)
                    uiSlots[i].SetItem(inventory[i].data);
                else
                    uiSlots[i].ClearSlot();
            }
        }
       
        private void UpdateHighlight()
        {
            for (int i = 0; i < uiSlots.Count; i++)
            {
                uiSlots[i].highlight.SetActive(i == currentIndex);
                
            }
        }
        private bool pickedItem = false;
        private bool justPickedUp = false; // NEW

        void HandlePickup()
        {
            if (heldObject != null)
            {
                if (inputInteract && pickedItem && !justPickedUp)
                {
                    DropObject();
                    InventoryManager.instance.SetInventoryEnabled(true);  // to re-enable
                    animator.SetBool("InterectHold", false);
                }
                else
                {
                    MoveHeldObject();
                }
            }

            inputInteract = false;
            justPickedUp = false; // reset after 1 frame
        }

        void PickUpObject(GameObject obj)
        {
            heldObject = obj;
            heldRB = heldObject.GetComponent<Rigidbody>();
            if (heldRB != null)
            {

                heldRB.useGravity = false;
                heldRB.freezeRotation = true;
                heldRB.isKinematic = true; // Set to kinematic when picked up

                // Smooth out the movement by setting interpolation
                heldRB.interpolation = RigidbodyInterpolation.Interpolate; // Ensure smooth movement when moved manually

                Vector3 lookDirection = playerCamera.transform.forward;
                lookDirection.y = 0f; // Optional: Only rotate on Y axis if you don't want it tilted up/down
                heldObject.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
            pickedItem = true;
            justPickedUp = true; // Mark that we JUST picked up something


        }

        public void DropObject()
        {
            if (!pickedItem) return;

            if (heldRB != null)
            {
                heldRB.useGravity = true;
                heldRB.freezeRotation = false;
                heldRB.isKinematic = false;
                heldRB.linearVelocity = Vector3.zero; // 🛠️ Zero the velocity before throwing
            }

            heldObject = null;
            heldRB = null;
            pickedItem = false;
        }
        [SerializeField] private LayerMask wallLayerMask;

        private Vector3 smoothedTargetPosition;

        void MoveHeldObject()
        {
            // Flat forward
            Vector3 flatForward = playerCamera.transform.forward;
            flatForward.y = 0f;
            flatForward.Normalize();

            // Target position
            Vector3 targetPos = playerCamera.transform.position + flatForward * hoverDistance;

           
            // Pitch for height adjustment
            float pitch = playerCamera.transform.localEulerAngles.x;
            if (pitch > 180f) pitch -= 360f;
            float heightAdjustment = Mathf.Clamp(-pitch / 10f, 0f, 8f);
            targetPos.y += heightAdjustment;

            // Smooth the target position (for stable motion)
            float smoothSpeed = 20f;
            smoothedTargetPosition = Vector3.Lerp(smoothedTargetPosition, targetPos, Time.fixedDeltaTime * smoothSpeed);

            // Move with smoothing
            Vector3 direction = smoothedTargetPosition - heldObject.transform.position;
            float moveDistance = direction.magnitude;

            // Check if the move path would collide with a wall
            if (Physics.Raycast(heldObject.transform.position, direction.normalized, out RaycastHit hitInfo, moveDistance, wallLayerMask))
            {
                // Call your custom method here
                DropObject();
                InventoryManager.instance.SetInventoryEnabled(true);  // to re-enable
                animator.SetBool("InterectHold", false);
                return; // Stop movement if wall hit
            }

            //// Rotate object to always face the camera
            //Vector3 lookDirection = flatForward;
            //if (lookDirection != Vector3.zero)
            //{
            //    Quaternion targetRot = Quaternion.LookRotation(
            //        heldObject.CompareTag("Customer") ? -lookDirection : lookDirection);

            //    heldObject.transform.rotation = Quaternion.Slerp(
            //        heldObject.transform.rotation,
            //        targetRot,
            //        Time.fixedDeltaTime * 10f // ✅ Fix here
            //    );
            //}


            if (moveDistance < 0.01f)
            {
                heldRB.linearVelocity = Vector3.zero;
                return;
            }

            Vector3 step = direction.normalized * pickupMoveSpeed * Time.fixedDeltaTime;

            if (step.magnitude > moveDistance)
                step = direction;

            heldRB.MovePosition(heldObject.transform.position + step);
        }


        public void SetInventoryEnabled(bool enabled)
        {
            inventoryEnabled = enabled;

            if (!enabled)
            {
                // Hide all hands when inventory is disabled
                for (int i = 0; i < handModels.Length; i++)
                {
                    if (handModels[i] != null)
                        handModels[i].SetActive(false);
                }

                // Optional: disable animator states
                SetAnimatorStates();

            }
            else
            {
                // Reactivate correct hand if inventory was re-enabled
                UpdateHands();
            }
        }
        public GameObject inventoryCanvas;
        public GameObject PlayerModelVisual;
        public void SetInventoryCanvas(bool enabled)
        {
            inventoryCanvas.SetActive(enabled);
            PlayerModelVisual.SetActive(enabled);
            SetInventoryEnabled(enabled);
        }

        public void DisableBarrels()
        {
            if (BarrelsModels.Count != 0)
            {
                foreach (GameObject i in BarrelsModels)
                {
                    i.SetActive(false);
                }
            }

        }

        //Object placement Code

        [Header("References")]
        public Image progressCircle;

        [Header("Grab Settings")]
        public float rayDistance = 3f;
        public float holdTimeThreshold = 1f;
        public float grabHeight = 1.5f;
        public float rotationSpeed = 100f;
        public Material validMaterial;
        public Material invalidMaterial;

        private GameObject grabbedObject;
        private float holdTimer;
        private bool isGrabbing;
        private bool isHoldingObject;

        private Vector3 grabOffset; public float grabDistance = 2f;
        private Renderer[] objectRenderers;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Vector3 lastMousePos;
        private Dictionary<Renderer, Material> originalMaterials = new();
        private Vector3 originalObjectHeight;
        private List<Collider> grabbedColliders = new(); private float currentGrabDistance;
        public float mouseSensitivity = 0.01f; // tweak this value for faster/slower movement
        private bool isObjectColliding = false; private Collider grabbedCollider;

        private GrabbedObjectCollisionChecker collisionChecker;
        void HandleRaycastHold(RaycastHit hit)
        {
            if (Input.GetMouseButton(0))
            {

                holdTimer += Time.deltaTime;
                progressCircle.fillAmount = holdTimer / holdTimeThreshold;

                if (holdTimer >= holdTimeThreshold)
                {
                    GrabObject(hit.collider.gameObject);
                    holdTimer = 0;
                    progressCircle.fillAmount = 0;
                }

            }
            else
            {
                holdTimer = 0;
                progressCircle.fillAmount = 0;
            }
        }

        void GrabObject(GameObject obj)
        {
            grabbedObject = obj;
            isHoldingObject = true;

            // Add or get the collision checker component
            collisionChecker = grabbedObject.GetComponent<GrabbedObjectCollisionChecker>();
            if (collisionChecker == null)
            {
                collisionChecker = grabbedObject.AddComponent<GrabbedObjectCollisionChecker>();
            }

            originalObjectHeight = grabbedObject.transform.position;
            currentGrabDistance = Vector3.Distance(playerCamera.transform.position, originalObjectHeight);
            inventoryEnabled = false;



            // Store and set colliders to trigger
            grabbedColliders.Clear();
            grabbedColliders.AddRange(grabbedObject.GetComponentsInChildren<Collider>());
            foreach (var col in grabbedColliders)
            {
                col.isTrigger = true;
            }

            // Set rigidbody kinematic
            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            if (rb) rb.isKinematic = true;

            // Store and set material
            objectRenderers = grabbedObject.GetComponentsInChildren<Renderer>();
            originalMaterials.Clear();
            foreach (var r in objectRenderers)
            {
                if (r != null && r.material != null)
                    originalMaterials[r] = r.material;
            }

            SetMaterial(validMaterial);

            // Set fixed grab height
            originalObjectHeight = grabbedObject.transform.position;
            lastMousePos = Input.mousePosition;

            // Optional: prevent self-collision during grab
            Physics.IgnoreLayerCollision(gameObject.layer, grabbedObject.layer, true);
        }

        [Header("Grab Offset (to align with crosshair)")]
        public Vector3 grabOffsets = Vector3.zero; // Editable in Inspector

        void FollowCamera()
        {
            if (grabbedObject == null) return;

            // 1. Get center of screen
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

            // 2. Create a ray from camera through screen center
            Ray ray = playerCamera.ScreenPointToRay(screenCenter);

            // 3. Move object along that ray direction at controlled distance
            Vector3 targetPos = ray.origin + ray.direction.normalized * currentGrabDistance;

            // 4. Lock Y position to original grab height
            targetPos.y = originalObjectHeight.y;

            // 5. Move the object
            grabbedObject.transform.position = targetPos;

            // 6. Adjust distance with vertical mouse input
            float verticalInput = Input.GetAxis("Mouse Y");
            float sensitivity = 0.1f;
            currentGrabDistance += verticalInput * sensitivity;
            currentGrabDistance = Mathf.Clamp(currentGrabDistance, 0.5f, 10f);
        }


        void HandleRotation()
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f && grabbedObject != null)
            {
                Transform parent = grabbedObject.transform.root;
                parent.Rotate(Vector3.up, scroll * rotationSpeed * Time.deltaTime * 20f, Space.World);
            }
        }

        void CheckCollisionAndSetMaterial()
        {
 
            if (collisionChecker == null && !buildingItem)
            {
                SetMaterial(validMaterial);
            }

            if (collisionChecker == null && buildingItem)
            {
                SetMaterial(buildMaterial);
            }

            isObjectColliding = collisionChecker.isColliding;
            if(!buildingItem)
                SetMaterial(isObjectColliding ? invalidMaterial : validMaterial);
            else
                SetMaterial(isObjectColliding ? invalidMaterial : buildMaterial);
        }


        void SetMaterial(Material mat)
        {
            foreach (Renderer rend in objectRenderers)
            {
                if (rend != null)
                    rend.material = mat;
            }
        }

        void RestoreOriginalMaterial()
        {
            if (!buildingItem)
            {
                foreach (var kvp in originalMaterials)
                {
                    if (kvp.Key != null && kvp.Value != null)
                        kvp.Key.material = kvp.Value;
                }
            }
            else
            {
                foreach (var kvp in originalMaterials)
                {
                    if (kvp.Key != null && kvp.Value != null)
                        kvp.Key.material = buildMaterial;
                }

            }
           
        }

        void PlaceObject()
        {
            if (isObjectColliding)
            {
                Debug.Log("Can't place object while colliding.");
                return;
            }

            // Restore colliders
            foreach (var col in grabbedColliders)
            {
                if (col != null)
                    col.isTrigger = false;
            }

            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true;

            RestoreOriginalMaterial();
            Physics.IgnoreLayerCollision(gameObject.layer, grabbedObject.layer, false);
            grabbedObject.layer = 12;
            Destroy(grabbedObject.GetComponent<GrabbedObjectCollisionChecker>());
            if (buildingItem)
                grabbedObject.GetComponent<CraftableItem>().Canvas.SetActive(true);

            grabbedObject = null;
            isHoldingObject = false; inventoryEnabled = true;
            buildingItem = false;
           

        }

        //Build System
        [Header("Build Settings")]
        public GameObject BuildCanvas;
        public GameObject BuildPostProcessing;
        public bool buildMenuOpen;
        public bool buildingItem;
        public List<GameObject> CraftableItems;
        public Material buildMaterial;
        [SerializeField] private float forwardDistance = 3f; // How far in front of the player to spawn
        [SerializeField] private float raycastHeight = 5f; // How high above the ground to start the ray
        [SerializeField] private LayerMask groundLayer; // LayerMask for ground (assign in inspector)

        public void AcessBuildMenu()
        {

            if(Input.GetKeyDown(KeyCode.Tab))
            {
                if (!buildMenuOpen)
                {
                    BuildCanvas.SetActive(true);
                    BuildPostProcessing.SetActive(true);
                    GameManager.Instance.EnableMouseCursor();
                   firstPersonMovementInput.restrictMovement = true;
                    inventoryEnabled = false;
                    buildMenuOpen = true;
                    
                }
                else
                {
                    BuildCanvas.SetActive(false);
                    BuildPostProcessing.SetActive(false);
                    GameManager.Instance.DisableMouseCursor();
                    firstPersonMovementInput.restrictMovement = false;
                    inventoryEnabled = true;
                    buildMenuOpen = false;
                }
               
            }
        }
        GameObject tempBuildObject;
        public void BuildItem(int index)
        {
            buildingItem = true;
            BuildCanvas.SetActive(false);
            BuildPostProcessing.SetActive(false);
            GameManager.Instance.DisableMouseCursor();
            firstPersonMovementInput.restrictMovement = false;       
            buildMenuOpen = false;

          

            // Calculate forward point
            Vector3 forwardPoint = playerCamera.transform.position + playerCamera.transform.forward * forwardDistance;

            // Raycast from above the target point down to find ground
            Vector3 rayOrigin = new Vector3(forwardPoint.x, forwardPoint.y + raycastHeight, forwardPoint.z);

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastHeight * 2f, groundLayer))
            {
                // Calculate object height
                float objectHeight = 0f;

                if (CraftableItems[index].TryGetComponent<Renderer>(out var renderer))
                {
                    objectHeight = renderer.bounds.size.y;
                }
                else if (CraftableItems[index].TryGetComponent<Collider>(out var collider))
                {
                    objectHeight = collider.bounds.size.y;
                }

                // Offset the spawn position upward so it sits *on* the ground
                Vector3 spawnPos = hit.point + Vector3.up * (objectHeight / 2f);

                GameObject obj = Instantiate(CraftableItems[index], spawnPos, Quaternion.identity);
                tempBuildObject = obj;
                grabbedObject = obj;
                isHoldingObject = true;


                if (collisionChecker == null)
                {
                    collisionChecker = grabbedObject.AddComponent<GrabbedObjectCollisionChecker>();
                }

                // Store and set colliders to trigger
                grabbedColliders.Clear();
                grabbedColliders.AddRange(grabbedObject.GetComponentsInChildren<Collider>());
                foreach (var col in grabbedColliders)
                {
                    col.isTrigger = true;
                }

                // Set rigidbody kinematic
                Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
                if (rb) rb.isKinematic = true;

                // Store and set material
                objectRenderers = grabbedObject.GetComponentsInChildren<Renderer>();
                originalMaterials.Clear();
                foreach (var r in objectRenderers)
                {
                    if (r != null && r.material != null)
                        originalMaterials[r] = r.material;
                }

                SetMaterial(buildMaterial);

                // Set fixed grab height
                originalObjectHeight = grabbedObject.transform.position;
                lastMousePos = Input.mousePosition;

                // Optional: prevent self-collision during grab
                Physics.IgnoreLayerCollision(gameObject.layer, grabbedObject.layer, true);

            }
            else
            {
                Debug.LogWarning("Ground not found when trying to spawn the object.");
            }

        }

        //Gun Logic

        public enum GunType { Revolver, Shotgun }

        [Header("Gun Components")]
        public GunType currentGun = GunType.Revolver;

        public Transform FirePoint;
        public Transform firePointShortGun;
        public Transform firePointGun;
        public GameObject bulletPrefab;
        public float bulletSpeed = 50f;
        public int maxAmmo = 6;
        public float reloadTime = 2f;
        
        [Header("Shotgun Settings")]
        public int pellets = 8;
        public float spreadAngle = 7f;

        [Header("Recoil")]
        public float recoilKickback = 2f;
        public float recoilRecoverSpeed = 5f;

        public int currentAmmo;
        private bool isReloading = false;
        private Vector3 originalCamPosition;

        private float nextTimeToFire = 0f;


        public GameObject muzzleFlash;
      
        void Shoot()
        {
            currentAmmo--;
            inventory[currentIndex].data.CurrentBullets = currentAmmo;
           
            UpdateAmmoUI();

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // center of screen
            Vector3 targetPoint;

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.GetPoint(100f); // shoot forward if nothing hit
            }

            Vector3 direction = (targetPoint - FirePoint.position).normalized;

            if (currentGun == GunType.Revolver)
            {
                direction.Normalize();
                FireBullet(direction);
                Instantiate(muzzleFlash, FirePoint.position, Quaternion.LookRotation(direction));
                SFXManager.Instance.PlaySFX("Firearm/GunFire", 0.5f);




            }
            else if (currentGun == GunType.Shotgun)
            {
                 SFXManager.Instance.PlaySFX("Firearm/ShortgunFire", 0.5f);
               
                
                Instantiate(muzzleFlash, FirePoint.position, Quaternion.LookRotation(direction));
                for (int i = 0; i < pellets; i++)
                {
                    Vector3 spreadDir = direction;
                    spreadDir += UnityEngine.Random.insideUnitSphere * Mathf.Tan(spreadAngle * Mathf.Deg2Rad);
                    spreadDir.Normalize();
                    FireBullet(spreadDir);
                }
            }

            //ApplyRecoil();
            firstPersonMovementInput.TriggerGunBob(1.6f, 1f, 0.2f); // Example values

            if (currentAmmo <= 0 && !isReloading)
            {
                StartCoroutine(Reload(0.9f));
            }

        }
        void FireBullet(Vector3 direction)
        {
            GameObject bullet = Instantiate(bulletPrefab, FirePoint.position, Quaternion.LookRotation(direction));
            
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * bulletSpeed;
            }

            Debug.DrawRay(FirePoint.position, direction * 100f, Color.yellow, 1f);
        }


      
     
        IEnumerator Reload(float time)
        {
            animator.SetTrigger("GunReload");

            isReloading = true;
            yield return new WaitForSeconds(time);
            if (inventory[currentIndex] != null && inventory[currentIndex].data.isGun)
            {
                if (inventory[currentIndex].data.name == "Gun")
                    SFXManager.Instance.PlaySFX("Firearm/GunReload", 0.4f);

                if (inventory[currentIndex].data.name == "Shortgun")
                    SFXManager.Instance.PlaySFX("Firearm/ShortgunReload", 0.4f);
            }
               

            yield return new WaitForSeconds(reloadTime - 1);
            if(inventory[currentIndex] != null && inventory[currentIndex].data.isGun)
            {
                
                inventory[currentIndex].data.TotalBullets -= inventory[currentIndex].data.magSize - currentAmmo;
                currentAmmo = inventory[currentIndex].data.magSize;
                maxAmmo = inventory[currentIndex].data.TotalBullets;
                inventory[currentIndex].data.CurrentBullets = inventory[currentIndex].data.magSize;
            }
               
            UpdateAmmoUI();
            isReloading = false;
        }

        void UpdateAmmoUI()
        {
            if (selectedItemName)
            {
                selectedItemName.text = $"{inventory[currentIndex].data.itemName} {currentAmmo}/{maxAmmo}";
            }
        }

    }
}

