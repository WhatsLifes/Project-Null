using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Camera cam;

    [Header("Weapon Models")]
    public GameObject meleeWeapon1;
    public GameObject meleeWeapon2;
    public GameObject rangedWeapon;

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
        SwitchWeapon(1);
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
        if (Input.GetButtonDown("Fire1") && Time.time >= nextAttackTime)
        {
            Attack();
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
        Debug.Log("Melee attack triggered!");

        // Trigger animation - animation event will handle damage
        Animator anim = weapon.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }
        else
        {
            Debug.LogWarning("No Animator found on weapon!");
        }

        nextAttackTime = Time.time + cooldown;
    }

    void RangedAttack(int damage, float range, float cooldown)
    {
        Debug.Log("Ranged attack!");

        // Trigger animation if gun has one
        Animator anim = rangedWeapon.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Attack");
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
    }

    // Called by animation event (via WeaponEventForwarder on the weapon)
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
        }
        else
        {
            Debug.Log("Animation Event: Missed - no enemy in range");
        }
    }
}