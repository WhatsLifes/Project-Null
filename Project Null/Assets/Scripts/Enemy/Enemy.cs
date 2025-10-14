using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    private Animator anim;

    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    private int health;
    public int damage = 5;

    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;
    public Player THEMC;

    [Header("Patrol")]
    public Vector3 walkPoint;
    public float walkPointRange = 10f;
    private bool walkPointSet;

    [Header("Combat")]
    public float timeBetweenAttacks = 5f;
    private bool alreadyAttacked;
    private Coroutine attackCooldownCoroutine;

    [Header("Detection")]
    public float sightRange = 10f;
    public float attackRange = 1f;
    public float stoppingDistance = 1f; // ADDED: Distance to stop before player
    public bool playerInSightRange;
    public bool playerInAttackRange;
    public bool isChasingPlayer = false;

    [Header("Activation")]
    private bool isActive = false;
    public bool canBeKilled = true;
    private bool isDead = false;

    private Rigidbody rb;
    private Vector3 lastValidPosition;

    private AudioSource audioSource;
    public AudioClip hitSound;
    [Range(0f, 1f)] public float volume = 1f;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            THEMC = playerObj.GetComponent<Player>();
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: Player not found!");
        }

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError($"{gameObject.name}: NavMeshAgent component missing!");
        }

        rb = GetComponent<Rigidbody>();

        // Initialize health
        health = maxHealth;

        // Start inactive
        if (agent != null)
        {
            agent.enabled = false;
        }
    }

    public void Activate()
    {
        this.enabled = true;

        if (isActive || agent == null) return;

        // Reset all state
        isActive = true;
        isDead = false;
        canBeKilled = true;
        health = maxHealth;
        alreadyAttacked = false;
        walkPointSet = false;
        isChasingPlayer = false;


        // Enable agent safely
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        agent.enabled = true;

        // Configure agent stopping behavior
        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = true;

        // Wait a frame for NavMesh to update, then verify
        StartCoroutine(VerifyNavMeshPlacement());

        Debug.Log($"{gameObject.name} Enemy AI activated - Health: {health}");
    }

    private IEnumerator VerifyNavMeshPlacement()
    {
        yield return null; // Wait one frame

        if (agent != null && agent.enabled && !agent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                lastValidPosition = hit.position;
                Debug.Log($"{gameObject.name} warped to NavMesh");
            }
            else
            {
                Debug.LogWarning($"{gameObject.name} could not find NavMesh! Deactivating.");
                Deactivate();
            }
        }
        else if (agent != null && agent.isOnNavMesh)
        {
            lastValidPosition = transform.position;
        }
    }

    public void Deactivate()
    {
        isActive = false;
        isChasingPlayer = false;
        walkPointSet = false;
        // Stop any ongoing attack cooldown
        if (attackCooldownCoroutine != null)
        {
            StopCoroutine(attackCooldownCoroutine);
            attackCooldownCoroutine = null;
        }

        if (agent != null && agent.enabled)
        {
            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
                agent.isStopped = true;
            }
            agent.enabled = false;
        }

        Debug.Log($"{gameObject.name} Enemy AI deactivated");
    }

    private void Update()
    {
        // Early exits
        if (!isActive || isDead || agent == null || !agent.enabled || player == null)
            return;

        // Safety check - if fell off NavMesh, try to recover
        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"{gameObject.name} fell off NavMesh! Attempting recovery...");

            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                lastValidPosition = hit.position;
            }
            else if (lastValidPosition != Vector3.zero)
            {
                agent.Warp(lastValidPosition);
            }
            else
            {
                Deactivate();
            }
            return;
        }

        lastValidPosition = transform.position;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        playerInSightRange = distanceToPlayer <= sightRange;
        playerInAttackRange = distanceToPlayer <= attackRange;

        // State machine
        if (!playerInSightRange)
        {
            Patrolling();
        }
        else if (playerInSightRange && !playerInAttackRange)
        {
            ChasePlayer();
        }
        else if (playerInAttackRange )
        {
            AttackPlayer();
        }
    }

    private void Patrolling()
    {
        if (!agent.isOnNavMesh) return;

        isChasingPlayer = false;
        agent.isStopped = false;
        agent.updateRotation = true;
        agent.stoppingDistance = 0f; // No stopping distance when patrolling

        if (!walkPointSet)
            SearchWalkPoint();

        if (walkPointSet)
        {
            // Only set destination if we're not already going there
            if ((agent.destination - walkPoint).sqrMagnitude > 0.1f)
            {
                agent.SetDestination(walkPoint);
            }
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        int maxAttempts = 5;
        for (int i = 0; i < maxAttempts; i++)
        {
            float randomZ = Random.Range(-walkPointRange, walkPointRange);
            float randomX = Random.Range(-walkPointRange, walkPointRange);

            Vector3 randomPoint = new Vector3(
                transform.position.x + randomX,
                transform.position.y,
                transform.position.z + randomZ
            );

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 5f, NavMesh.AllAreas))
            {
                // Verify path is complete
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    walkPoint = hit.position;
                    walkPointSet = true;
                    return;
                }
            }
        }

        // If no valid point found, try again next frame
        walkPointSet = false;
    }

    private void ChasePlayer()
    {
        if (!agent.isOnNavMesh || player == null) return;

        isChasingPlayer = true;
        agent.isStopped = false;
        agent.updateRotation = true;
        agent.stoppingDistance = stoppingDistance; // Stop before reaching player

        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Only update destination if player moved significantly OR we're far from them
        if (distanceToPlayer > stoppingDistance + 0.5f)
        {
            if ((agent.destination - player.position).sqrMagnitude > 1f)
            {
                agent.SetDestination(player.position);
            }
        }
        else
        {
            // We're close enough, stop moving
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    public void AttackPlayer()
    {
        if (!agent.isOnNavMesh || player == null || THEMC == null) return;

        // Ensure we're stopped
        if (!agent.isStopped)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero; // Force stop velocity
        }
        


        // Manual rotation
        agent.updateRotation = false;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }


        // Attack
        if (!alreadyAttacked && attackCooldownCoroutine == null)
        {
            anim.SetTrigger("isAttacking");

            Debug.Log($"{gameObject.name} attacked the player for {damage} damage");

            THEMC.TakeDamage(damage);

            alreadyAttacked = true;
            attackCooldownCoroutine = StartCoroutine(AttackCooldown());
            
        }


    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(timeBetweenAttacks);
        alreadyAttacked = false;
        attackCooldownCoroutine = null;
    }

    public void TakeDamage(int damageAmount)
    {
        if (!canBeKilled || isDead)
        {
            Debug.Log($"{gameObject.name} is protected from damage");
            return;
        }

        audioSource.PlayOneShot(hitSound, volume);

        Debug.Log($"{gameObject.name} took {damageAmount} damage. Health: {health} -> {health - damageAmount}");
        health -= damageAmount;

        if (health <= 0)
        {
            anim.SetBool("isMoving", false);
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"{gameObject.name} died");

        DollBehavior dollBehavior = GetComponent<DollBehavior>();
        if (dollBehavior != null)
        {
            dollBehavior.Down();
        }
        else
        {
            DestroyEnemy();
        }
    }

    public void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    public void RagdollDeath()
    {
        if (isDead == false)
            isDead = true;

        isActive = false;

        // Stop all coroutines
        if (attackCooldownCoroutine != null)
        {
            StopCoroutine(attackCooldownCoroutine);
            attackCooldownCoroutine = null;
        }

        // Disable agent safely
        if (agent != null && agent.enabled)
        {
            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            agent.enabled = false;
        }

        // Enable physics
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            rb.angularDamping = 5f;

            // Gentle backward tilt/rotation (not flying force)
            Vector3 torque = new Vector3(
                Random.Range(-2f, -4f),  // Negative X = backward tilt
                Random.Range(-1f, 1f),   // Slight twist
                Random.Range(-1f, 1f)    // Slight side tilt
            );
            rb.AddTorque(torque, ForceMode.Impulse);
        }

        // Disable this script to stop Update loop
        this.enabled = false;
    }

    // Handle collisions with player or obstacles
    private void OnCollisionEnter(Collision collision)
    {
        if (!isActive || !agent.enabled || !agent.isOnNavMesh) return;

        // If we collide with player while chasing, enter attack mode
        if (collision.gameObject.CompareTag("Player") && isChasingPlayer)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!isActive || !agent.enabled || !agent.isOnNavMesh) return;

        // Keep agent stopped while colliding with player
        if (collision.gameObject.CompareTag("Player"))
        {
            agent.velocity = Vector3.zero;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, walkPointRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }
}