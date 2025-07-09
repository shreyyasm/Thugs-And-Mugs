using Cinemachine;
using Dhiraj;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Shreyas
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonMovementInputSystem : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float walkSpeed = 5f;
        public float runSpeed = 9f;
        public float jumpForce = 5f;
        public float gravity = -9.81f;

        [Header("Crouch Settings")]
        public float crouchHeight = 1f;
        public float standingHeight = 2f;
        public float crouchSpeed = 2.5f;
        public float crouchCamY = 0.5f;
        public float standCamY = 0.9f;
        public float cameraMoveSpeed = 5f;
        private bool isCrouching = false;

        [Header("References")]
        public Transform playerCamera; // Assign this to the actual camera in the Inspector
        public Animator animator;
        public InventoryManager inventoryManager;

        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;

        private Vector2 inputMovement;
        private bool inputJump;
        private bool inputRun;
        private bool inputDefence;
        private bool wasKeyJustReleased = false;

        private PlayerControls inputActions;
        public bool playerBusy;
        public bool restrictMovement;
        public bool isBlocking;

        public float punchCooldown = 0.5f;
        private bool punchAnimToggle = false;
        private bool canPunch = true;


       

    [Header("Footstep Settings")]
        public float walkStepInterval = 0.5f;
        public float runStepInterval = 0.3f;
        public float crouchStepInterval = 0.8f;

        private float stepTimer = 0f;
        private string currentFloorType = "Default";


        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            inputActions = new PlayerControls();

            inputActions.Player.Move.performed += ctx => inputMovement = ctx.ReadValue<Vector2>();
            inputActions.Player.Move.canceled += _ => inputMovement = Vector2.zero;

            inputActions.Player.Jump.performed += _ => inputJump = true;
            inputActions.Player.Run.performed += _ =>
            {
                if (CanRun())
                {
                    inputRun = true;
                    ChangeFOV(70f, 0.2f);

                }
            };

            inputActions.Player.Run.canceled += _ =>
            {
                inputRun = false;
                ChangeFOV(60f, 0.2f);

            };



            inputActions.Player.Crouch.performed += _ => ToggleCrouch();
            inputActions.Player.Punch.performed += _ => Punch();

            inputActions.Player.Defence.started += _ => inputDefence = true;
            inputActions.Player.Defence.canceled += _ =>
            {
                inputDefence = false;
                wasKeyJustReleased = true;
            };
        }
        private void Start()
        {
            if (virtualCam != null)
                perlin = virtualCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            if (volume != null && volume.profile.TryGet(out vignette))
            {
                // Good
            }
            else
            {
                Debug.LogWarning("Vignette not found or Volume is null.");
            }
        }
        private void OnEnable() => inputActions.Enable();
        private void OnDisable() => inputActions.Disable();

        private void Update()
        {
            if (playerBusy || restrictMovement) return;

            HandleMovement();
            HandleBlocking();
            HandleCameraHeight();
            CheckGroundSurface();
            HandleFootsteps();
            UpdateHeadBob();


        }
        private bool CanRun()
        {
            // Only allow running when:
            // - Not crouching
            // - Player is moving
            // - Player is not moving backward (inputMovement.y >= 0)
            return !isCrouching && inputMovement.magnitude > 0.1f && inputMovement.y >= 0.1f;
        }


        private void HandleMovement()
        {
            isGrounded = controller.isGrounded;

            if (isGrounded && velocity.y < 0)
                velocity.y = -2f;

            Vector3 move = (transform.right * inputMovement.x + transform.forward * inputMovement.y).normalized;
            // Re-check in case you started standing still or crouched mid-run
            if (!CanRun())
            {
                inputRun = false;
                ChangeFOV(60f, 0.2f);

            }

            float currentSpeed = isCrouching ? crouchSpeed : (inputRun ? runSpeed : walkSpeed);
    

            controller.Move(move * currentSpeed * Time.deltaTime);

            if (isGrounded && inputJump)
            {
                SFXManager.Instance.PlaySFX($"Player/JumpStart{currentFloorType}", 1f);
                if (isCrouching)
                {
                    ToggleCrouch(); // stand up instead
                }
                else
                {
                    
                    velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
                    
                    string sfxKey = $"Player/JumpEnd{currentFloorType}";
                    LeanTween.delayedCall(0.7f, () =>
                    {
                        SFXManager.Instance.PlaySFX(sfxKey, 0.5f);
                    });
                   
                }
            }

            inputJump = false;

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
        
        private void HandleFootsteps()
        {
            if (!controller.isGrounded) return;
            if (inputMovement.magnitude < 0.1f) return; // not moving

            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                CheckGroundSurface();
                PlayFootstepSFX();

                // Set next interval based on state
                if (isCrouching)
                    stepTimer = crouchStepInterval;
                else if (inputRun && CanRun())
                    stepTimer = runStepInterval;
                else
                    stepTimer = walkStepInterval;
            }
        }

        private void PlayFootstepSFX()
        {
            string sfxKey = $"Player/Footstep{currentFloorType}";
            SFXManager.Instance.PlaySFX(sfxKey, 1f);
        }


        private void CheckGroundSurface()
        {
            Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 2f))
            {
                if (hit.collider.CompareTag("FloorWood"))
                    currentFloorType = "Wood";
                else if (hit.collider.CompareTag("FloorConcrete"))
                    currentFloorType = "Concrete";
                else
                    currentFloorType = "Default";
            }
        }

        private void ToggleCrouch()
        {
            isCrouching = !isCrouching;

            if (isCrouching)
            {
                controller.height = crouchHeight;
                controller.center = new Vector3(0, crouchHeight / 2f, 0);

                // For vignette fade-in
                ChangeVignetteSmooth(0.4f, 0.3f);

                SFXManager.Instance.PlaySFX("Player/Crouch", 0.5f);
            }
            else
            {
                controller.height = standingHeight;
                controller.center = new Vector3(0, standingHeight / 2f, 0);
                // For vignette fade-out
                ChangeVignetteSmooth(0f, 0.3f);
                SFXManager.Instance.PlaySFX("Player/Crouch", 0.5f);

            }

           
        }

        private void HandleCameraHeight()
        {
            if (playerCamera == null) return;

            Vector3 localPos = playerCamera.localPosition;
            float targetY = isCrouching ? crouchCamY : standCamY;
            localPos.y = Mathf.Lerp(localPos.y, targetY, Time.deltaTime * cameraMoveSpeed);
            playerCamera.localPosition = localPos;
        }

        private void HandleBlocking()
        {
            if (inputDefence)
            {
                if (!isBlocking)
                {
                    isBlocking = true;
                    animator.SetBool("IsBlocking", true);
                    animator.SetBool("InterectHold", false);

                    inventoryManager.SetInventoryEnabled(false);
                    inventoryManager.DropObject();
                    inventoryManager.HideInteractSign();
                    inventoryManager.ClearLastOutlined();
                }
            }
            else if (wasKeyJustReleased)
            {
                if (isBlocking)
                {
                    isBlocking = false;
                    animator.SetBool("IsBlocking", false);
                    inventoryManager.SetInventoryEnabled(true);
                }
                wasKeyJustReleased = false;
            }
        }

        public void Punch()
        {
            if(!inventoryManager.holdingSomething)
            {
                if (playerBusy || !canPunch || restrictMovement) return;

                animator.SetBool("IsPunching", true);

                if (punchAnimToggle)
                {
                    animator.SetTrigger("Punch1");
                    animator.ResetTrigger("Punch2");
                    SFXManager.Instance.PlaySFX($"Player/PunchThrow", 0.6f);
                }
                else
                {
                    animator.SetTrigger("Punch2");
                    animator.ResetTrigger("Punch1");
                    SFXManager.Instance.PlaySFX($"Player/PunchThrow", 0.6f);
                }

                punchAnimToggle = !punchAnimToggle;
                StartCoroutine(PunchCooldown());
            }
            
        }

        private IEnumerator PunchCooldown()
        {
            canPunch = false;
            yield return new WaitForSeconds(punchCooldown);
            animator.SetBool("IsPunching", false);
            canPunch = true;
        }

        //Camera Sprint FOV change
        private Coroutine fovRoutine;

        public void ChangeFOV(float targetFOV, float duration)
        {
            if (fovRoutine != null)
                StopCoroutine(fovRoutine);

            fovRoutine = StartCoroutine(SmoothFOVChange(targetFOV, duration));
        }

        private IEnumerator SmoothFOVChange(float targetFOV, float duration)
        {
            float startFOV = virtualCam.m_Lens.FieldOfView;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                virtualCam.m_Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, elapsed / duration);
                yield return null;
            }

            virtualCam.m_Lens.FieldOfView = targetFOV;
            fovRoutine = null;
        }

        //Player Post Processing
        public Volume volume; // Assign the volume in inspector
        private Vignette vignette;
        private Coroutine vignetteRoutine;
        public void ChangeVignetteSmooth(float targetIntensity, float duration)
        {
            if (vignetteRoutine != null)
                StopCoroutine(vignetteRoutine);

            vignetteRoutine = StartCoroutine(SmoothVignette(targetIntensity, duration));
        }

        private IEnumerator SmoothVignette(float target, float time)
        {
            float start = vignette.intensity.value;
            float elapsed = 0f;

            while (elapsed < time)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / time);
                vignette.intensity.value = Mathf.Lerp(start, target, t);
                yield return null;
            }

            vignette.intensity.value = target;
            vignetteRoutine = null;
        }

        [Header("Headbob (Run Only)")]
        public CinemachineVirtualCamera virtualCam;
        private CinemachineBasicMultiChannelPerlin perlin;

        public float runAmplitude = 1f;
        public float runFrequency = 2f;
        public float bobTransitionSpeed = 4f;
        private void UpdateHeadBob()
        {
            if (perlin == null) return;

            // Only apply headbob when running and grounded
            bool isRunning = inputRun && CanRun() && controller.isGrounded;

            float targetAmplitude = isRunning ? runAmplitude : 0f;
            float targetFrequency = isRunning ? runFrequency : 0f;

            perlin.m_AmplitudeGain = Mathf.Lerp(perlin.m_AmplitudeGain, targetAmplitude, Time.deltaTime * bobTransitionSpeed);
            perlin.m_FrequencyGain = Mathf.Lerp(perlin.m_FrequencyGain, targetFrequency, Time.deltaTime * bobTransitionSpeed);
        }

    }
}
