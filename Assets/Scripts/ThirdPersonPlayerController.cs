using Cinemachine;
using StarterAssets;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using static UnityEngine.LowLevelPhysics2D.PhysicsBody;

public class ThirdPersonPlayerController : MonoBehaviour
{
    [Header("Rig & Camera References")]
    [SerializeField] private Rig aimRig;
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    [SerializeField] private float normalSensitivity;
    [SerializeField] private float aimSensitivity;
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();

    [Header("Effects & Debug")]
    [SerializeField] private Transform debugTransform;
    [SerializeField] private Transform enemyHit;
    [SerializeField] private Transform enemyMiss;
    [SerializeField] private ParticleSystem muzzleFlash;

    [Header("Animation Constraints")]
    [SerializeField] private MultiAimConstraint bodyConstraint;
    [SerializeField] private MultiAimConstraint headConstraint;
    [SerializeField] private MultiAimConstraint aimConstraint;

    [Header("Movement & Combat")]
    [SerializeField] private float aimSprintSpeed = 2.0f;
    [SerializeField] private int damagePerShot = 1;

    private float originalSprintSpeed;
    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;
    private Animator animator;

    public GameObject gun;
    public AudioSource gunSound;

    private void Awake()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        Cursor.lockState = CursorLockMode.Locked;
        animator = GetComponent<Animator>();

        originalSprintSpeed = thirdPersonController.SprintSpeed;
    }

    private void Update()
    {
        Vector3 mouseWorldPosition = Vector3.zero;
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        Transform hitTransform = null;

        
        float defaultAimDistance = 100f;
        Vector3 defaultAimTarget = ray.origin + ray.direction * defaultAimDistance;

        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            if (debugTransform != null) debugTransform.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
            hitTransform = raycastHit.transform;
        }
        else
        {
            
            mouseWorldPosition = defaultAimTarget;
            if (debugTransform != null) debugTransform.position = defaultAimTarget;
        }

        
        if (starterAssetsInputs.aim)
        {
            thirdPersonController.SprintSpeed = aimSprintSpeed;
            aimVirtualCamera.gameObject.SetActive(true);
            thirdPersonController.SetSensitivity(aimSensitivity);
            thirdPersonController.SetRotateOnMove(false);
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 1f, Time.deltaTime * 10));

            
            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0f; 
            Vector3 aimDirection = cameraForward.normalized;

            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);

            headConstraint.weight = 1f;
            bodyConstraint.weight = 1f;
            aimConstraint.weight = 1f;

            gun.SetActive(true);
        }
        else
        {
            thirdPersonController.SprintSpeed = originalSprintSpeed;
            aimVirtualCamera.gameObject.SetActive(false);
            thirdPersonController.SetSensitivity(normalSensitivity);
            thirdPersonController.SetRotateOnMove(true);
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 10));

            headConstraint.weight = 0f;
            bodyConstraint.weight = 0f;
            aimConstraint.weight = 0f;

            gun.SetActive(false);
        }

        
        if (starterAssetsInputs.shoot)
        {
            if (starterAssetsInputs.aim)
            {
                if (hitTransform != null)
                {
                    if (muzzleFlash != null)
                    {
                        muzzleFlash.Play();
                        gunSound.Play();
                        animator.SetTrigger("Recoil");
                    }

                    
                    if (hitTransform.GetComponent<BulletTarget>() != null)
                    {
                        Instantiate(enemyHit, mouseWorldPosition, Quaternion.identity);
                        Debug.Log("Enemy Hit!");

                        
                        EnemyHealth enemyHealth = hitTransform.GetComponent<EnemyHealth>();
                        if (enemyHealth == null)
                        {
                            enemyHealth = hitTransform.GetComponentInParent<EnemyHealth>();
                        }

                        
                        if (enemyHealth != null)
                        {
                            enemyHealth.TakeDamage(damagePerShot);
                        }
                    }
                    else
                    {
                        Instantiate(enemyMiss, mouseWorldPosition, Quaternion.identity);
                    }
                }
                else
                {
                    
                    if (muzzleFlash != null)
                    {
                        muzzleFlash.Play();
                        gunSound.Play();
                        animator.SetTrigger("Recoil");
                    }
                    Instantiate(enemyMiss, defaultAimTarget, Quaternion.identity);
                }
            }

            starterAssetsInputs.shoot = false;
        }
    }
}