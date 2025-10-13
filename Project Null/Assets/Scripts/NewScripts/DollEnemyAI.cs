using UnityEngine;
using UnityEngine.AI;

public class DollEnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 3.5f;
    public float chaseRange = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;

    private NavMeshAgent agent;
    private Transform player;

    // private PlayerHealth playerHealth; // Disabled for now
    private float lastAttackTime;
    private bool isActive = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        agent.enabled = false; // Start disabled until activated
    }

    public void Activate()
    {
        if (isActive) return;

        isActive = true;
        agent.enabled = true;
        agent.speed = speed;

        Debug.Log($"{gameObject.name} is now hostile!");
    }

    /* void Update()
    {
        if (!isActive || player == null || !agent.enabled)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= chaseRange)
        {
            agent.SetDestination(player.position);

            // If close enough to attack
            if (distance <= attackRange && Time.time > lastAttackTime + attackCooldown)
            {
                AttackPlayer();
                lastAttackTime = Time.time;
            }
        }
    } */

    void AttackPlayer()
    {
        Debug.Log($"{gameObject.name} attacks the player!");
        // Uncomment these once you have your PlayerHealth system set up:
        // if (playerHealth == null)
        //     playerHealth = player.GetComponent<PlayerHealth>();
        //
        // if (playerHealth != null)
        //     playerHealth.TakeDamage(10f);
    }
}