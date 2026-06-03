using UnityEngine;
using Cinemachine;
using StarterAssets;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;

public class ThirdPersonPlayerController : MonoBehaviour
{
    [SerializeField] private Rig aimRig;
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    [SerializeField] private float normalSensitivity;
    [SerializeField] private float aimSensitivity;
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
    [SerializeField] private Transform debugTransform;
    [SerializeField] private Transform enemyHit;
    [SerializeField] private Transform enemyMiss;
    [SerializeField] private ParticleSystem muzzleFlash;

    // --- SPRINT SPEED SETTING ---
    [SerializeField] private float aimSprintSpeed = 2.0f; // Type your exact aiming sprint speed here
    private float originalSprintSpeed;

    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;
    private Animator animator;
    private float aimRigWeight;

    public GameObject gun; // Reverted back to GameObject


    private void Awake()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        Cursor.lockState = CursorLockMode.Locked;
        animator = GetComponent<Animator>();

        // Save the normal sprint speed from the Unity script before we change it
        originalSprintSpeed = thirdPersonController.SprintSpeed;
    }

    private void Update()
    {
        Vector3 mouseWorldPosition = Vector3.zero;

        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        Transform hitTransform = null;
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            debugTransform.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
            hitTransform = raycastHit.transform;
        }

        if (starterAssetsInputs.aim)
        {
            // Inject your custom speed into the Unity script while aiming
            thirdPersonController.SprintSpeed = aimSprintSpeed;

            aimVirtualCamera.gameObject.SetActive(true);
            thirdPersonController.SetSensitivity(aimSensitivity);
            thirdPersonController.SetRotateOnMove(false);
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 1f, Time.deltaTime * 10));

            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
            aimRigWeight = 1f;
            gun.SetActive(true);
        }
        else
        {
            // Restore the original speed when you stop aiming
            thirdPersonController.SprintSpeed = originalSprintSpeed;

            aimVirtualCamera.gameObject.SetActive(false);
            thirdPersonController.SetSensitivity(normalSensitivity);
            thirdPersonController.SetRotateOnMove(true);
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 10));
            aimRigWeight = 0f;
            gun.SetActive(false);
        }

        aimRig.weight = Mathf.Lerp(aimRig.weight, aimRigWeight, Time.deltaTime * 20f);


        if (starterAssetsInputs.shoot)
        {
            animator.SetTrigger("Recoil");

            if (starterAssetsInputs.aim)
            {
                if (hitTransform != null)
                {
                    if (muzzleFlash != null)
                    {
                        muzzleFlash.Play();
                    }

                    if (hitTransform.GetComponent<BulletTarget>() != null)
                    {
                        Instantiate(enemyHit, mouseWorldPosition, Quaternion.identity);
                        Debug.Log("Enemy Hit!");
                    }
                    else
                    {
                        Instantiate(enemyMiss, mouseWorldPosition, Quaternion.identity);
                    }
                }
            }

            starterAssetsInputs.shoot = false;
        }
    }
}