using System.Collections;
using UnityEngine;

public class TestWeapon : MonoBehaviour
{
    public GameObject Melee;
    private bool CanAttack = true;
    public float AttackCooldown = 1f;
    public bool IsAttacking = false;
    public float HitWindowDuration = 0.3f; 
    
    void Update()
    {
        // On left click
        if (Input.GetMouseButton(0))
        {
            // if can attack
            if (CanAttack)
            {
                // call attack
                MeleeAttack();
            }
        }
    }

    // Melee attack
    public void MeleeAttack()
    {
        
        print("attacked enemy");
        IsAttacking = true; 
        CanAttack = false;
        Animator anim = Melee.GetComponent<Animator>();
        anim.SetTrigger("Attack");
        StartCoroutine(ResetAttack());
        StartCoroutine(ResetIsAttacking()); 
    }

    IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(AttackCooldown);
        CanAttack = true;
    }

    IEnumerator ResetIsAttacking() 
    {
        // IsAttacking is only true for the active duration of the swing
        yield return new WaitForSeconds(HitWindowDuration); 
        IsAttacking = false; 
    }
    
}

