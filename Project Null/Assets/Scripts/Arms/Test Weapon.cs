using System.Collections;
using UnityEngine;

public class TestWeapon : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public GameObject Melee;
    public Animator anim;

    [Header("Weapon Models")]
    public GameObject meleeWeapon1;
    public GameObject meleeWeapon2;
    public GameObject rangedWeapon;

    [Header("Attack Settings")]
    private bool CanAttack = true;
    public float AttackCooldown = 1f;
    public bool IsAttacking = false;
    public float HitWindowDuration = 0.3f;

    [Header("Weapon 1 - Melee")]
    public int melee1Damage = 25;
    public float melee1Range = 2f;
    public float melee1Cooldown = 0.6f;

    [Header("Weapon 2 - Melee")]
    public int melee2Damage = 40;
    public float melee2Range = 2.5f;
    public float melee2Cooldown = 1.2f;

    [Header("Weapon 3 - Ranged")]
    public int rangedDamage = 20;
    public float rangedRange = 100f;
    public float rangedCooldown = 0.3f;

    private int currentWeapon = 1;
    private float nextAttackTime = 0f;

    void Start()
    {
        // Auto-find camera if not assigned
        if (cam == null)
        {
            cam = Camera.main;
        }

        SwitchWeapon(1); // Start with weapon 1
    }

    void Update()
    {
        // Weapon switching
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SwitchWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            SwitchWeapon(2);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            SwitchWeapon(3);

        // Attack
        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            if (CanAttack)
            {
                Attack();
            }
        }
    }

    void SwitchWeapon(int weaponNumber)
    {
        currentWeapon = weaponNumber;

        // Hide all weapons
        if (meleeWeapon1 != null) meleeWeapon1.SetActive(false);
        if (meleeWeapon2 != null) meleeWeapon2.SetActive(false);
        if (rangedWeapon != null) rangedWeapon.SetActive(false);

        // Show selected weapon
        switch (weaponNumber)
        {
            case 1:
                if (meleeWeapon1 != null) meleeWeapon1.SetActive(true);
                Debug.Log("Switched to Melee Weapon 1");
                break;
            case 2:
                if (meleeWeapon2 != null) meleeWeapon2.SetActive(true);
                Debug.Log("Switched to Melee Weapon 2");
                break;
            case 3:
                if (rangedWeapon != null) rangedWeapon.SetActive(true);
                Debug.Log("Switched to Ranged Weapon");
                break;
        }
    }

    void Attack()
    {
        switch (currentWeapon)
        {
            case 1:
                MeleeAttack(melee1Damage, melee1Range, melee1Cooldown, meleeWeapon1);
                break;
            case 2:
                MeleeAttack(melee2Damage, melee2Range, melee2Cooldown, meleeWeapon2);
                break;
            case 3:
                RangedAttack(rangedDamage, rangedRange, rangedCooldown);
                break;
        }
    }

    void MeleeAttack(int damage, float range, float cooldown, GameObject weapon)
    {
        print("attacking");
        IsAttacking = true;
        CanAttack = false;

        // Trigger animation - animation event will handle damage
        anim = weapon.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Attack");
            Debug.Log("Animation triggered on " + weapon.name);
        }
        else
        {
            Debug.LogWarning("No Animator found on weapon: " + weapon.name);
        }

        // Set attack cooldown
        nextAttackTime = Time.time + cooldown;
        AttackCooldown = cooldown; // Update for coroutines

        StartCoroutine(ResetAttack());
        StartCoroutine(ResetIsAttacking());
    }

    void RangedAttack(int damage, float range, float cooldown)
    {
        Debug.Log("Ranged attack!");
        IsAttacking = true;
        CanAttack = false;

        // Trigger animation if gun has one
        Animator gunAnim = rangedWeapon.GetComponentInChildren<Animator>();
        if (gunAnim != null)
        {
            gunAnim.SetTrigger("Attack");
        }

        // Ranged weapons deal damage immediately (no animation event needed)
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, range))
        {
            Enemy enemy = hit.transform.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"Ranged hit for {damage} damage!");
            }
        }

        nextAttackTime = Time.time + cooldown;
        AttackCooldown = cooldown;

        StartCoroutine(ResetAttack());
        StartCoroutine(ResetIsAttacking());
    }

    // Called by animation event
    public void DealDamage()
    {
        Debug.Log("DealDamage called by animation event!");

        int damage = 0;
        float range = 0;

        // Get current weapon stats
        switch (currentWeapon)
        {
            case 1:
                damage = melee1Damage;
                range = melee1Range;
                break;
            case 2:
                damage = melee2Damage;
                range = melee2Range;
                break;
            case 3:
                damage = rangedDamage;
                range = rangedRange;
                break;
        }

        // Perform raycast from camera
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, range))
        {
            Debug.Log("Animation Event: Hit " + hit.transform.name);

            Enemy enemy = hit.transform.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"Animation Event: Damaged enemy for {damage}!");
            }
            // Check if we hit a vent
            VentBreak vent = hit.transform.GetComponent<VentBreak>();
            if (vent != null)
            {
                vent.TakeDamage(damage);
                Debug.Log($"Animation Event: Hit vent for {damage} damage!");
            }
        
        }
        else
        {
            Debug.Log("Animation Event: Missed - no enemy in range");
        }
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