using UnityEngine;

public class TestEnemy : MonoBehaviour
{
    public Enemy EnemyAttack;

    public int damage;
    
    void Start()
    {
        EnemyAttack.AttackPlayer(damage);
    }

    
}
