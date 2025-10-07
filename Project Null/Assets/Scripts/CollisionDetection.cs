using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    public TestWeapon melee;
    public GameObject HitParticle;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collided object is an Enemy AND the player is currently attacking
        if (other.CompareTag("Enemy") && melee.IsAttacking)
        {
            // Get the Enemy component from the object that was actually hit.
            Enemy enemyHit = other.gameObject.GetComponent<Enemy>();
           
            
            enemyHit.TakeDamage(25); // Call TakeDamage on the enemy that was hit
            print("it works this time");
            Instantiate(HitParticle, new Vector3(transform.position.x, transform.position.y, other.transform.position.z), other.transform.rotation);
            
        }
    }
}