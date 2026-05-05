using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;

    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;

    public float viewDistance = 15f;
    public float fieldOfView = 60f;

    public Transform player;
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public float bulletSpeed = 50f;
    public float shootCooldown = 2f;

    private NavMeshAgent agent;
    private Animator animator;

    private Transform currentTarget;
    private bool hasSeenPlayer = false;
    private float shootTimer = 0f;

    public ParticleSystem muzzleFlash;

    public AudioSource shootAudio;
    public AudioClip shootClip;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        currentTarget = pointB;
        agent.speed = patrolSpeed;
        agent.SetDestination(currentTarget.position);
    }

    void Update()
    {
        shootTimer -= Time.deltaTime;

        if (!hasSeenPlayer && CanSeePlayer())
        {
            hasSeenPlayer = true;
            //animator?.SetBool("isChasingPlayer", true);
        }

        if (hasSeenPlayer)
        {
            ChasePlayer();

            if (CanSeePlayer())
                TryShoot();


        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentTarget = currentTarget == pointA ? pointB : pointA;
            agent.SetDestination(currentTarget.position);
        }
    }

    void ChasePlayer()
    {
        agent.speed = chaseSpeed;

        if (player != null)
            agent.SetDestination(player.position);
    }

    void TryShoot()
    {
        if (shootTimer <= 0f)
        {
            Shoot();
            shootTimer = shootCooldown;

        }
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = player.position - transform.position;
        float distance = directionToPlayer.magnitude;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (distance < viewDistance && angle < fieldOfView / 2f)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 1.5f;

            if (Physics.Raycast(rayOrigin, directionToPlayer.normalized, out RaycastHit hit, viewDistance))
            {
                if (hit.collider.CompareTag("Player"))
                    return true;
            }
        }

        return false;
    }

    void Shoot()
    {
        if (bulletPrefab == null || shootPoint == null || player == null) return;



        Vector3 targetPosition = player.position;
        targetPosition.y = shootPoint.position.y;

        Vector3 direction = (targetPosition - shootPoint.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.LookRotation(direction));
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = direction * bulletSpeed;
        muzzleFlash.Play();
        shootAudio.PlayOneShot(shootClip);
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 leftRay = Quaternion.Euler(0, -fieldOfView / 2, 0) * transform.forward;
        Vector3 rightRay = Quaternion.Euler(0, fieldOfView / 2, 0) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + Vector3.up * 1.5f, transform.position + leftRay * viewDistance);
        Gizmos.DrawLine(transform.position + Vector3.up * 1.5f, transform.position + rightRay * viewDistance);
    }
}