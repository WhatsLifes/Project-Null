using System.Collections;
using UnityEngine;

public class TestWeapon : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
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

    [Header("Debug")]
    public bool showDebugRay = true;
    public LayerMask dollLayer;

    private int currentWeapon = 1;
    private float nextAttackTime = 0f;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        SwitchWeapon(1);
    }

    void Update()
    {
        // Switch between weapons
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchWeapon(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchWeapon(3);

        // Attack input
        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            if (CanAttack) Attack();
        }
    }

    void SwitchWeapon(int weaponNumber)
    {
        currentWeapon = weaponNumber;
        if (meleeWeapon1 != null) meleeWeapon1.SetActive(false);
        if (meleeWeapon2 != null) meleeWeapon2.SetActive(false);
        if (rangedWeapon != null) rangedWeapon.SetActive(false);

        switch (weaponNumber)
        {
            case 1:
                if (meleeWeapon1 != null) meleeWeapon1.SetActive(true);
                break;
            case 2:
                if (meleeWeapon2 != null) meleeWeapon2.SetActive(true);
                break;
            case 3:
                if (rangedWeapon != null) rangedWeapon.SetActive(true);
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
        IsAttacking = true;
        CanAttack = false;

        // Play animation
        anim = weapon != null ? weapon.GetComponentInChildren<Animator>() : null;
        if (anim != null)
            anim.SetTrigger("Attack");

        nextAttackTime = Time.time + cooldown;
        AttackCooldown = cooldown;

        // Wait for animation event instead of hitting instantly
        StartCoroutine(ResetAttack());
        StartCoroutine(ResetIsAttacking());
    }

    void RangedAttack(int damage, float range, float cooldown)
    {
        IsAttacking = true;
        CanAttack = false;

        Animator gunAnim = rangedWeapon?.GetComponentInChildren<Animator>();
        if (gunAnim != null)
            gunAnim.SetTrigger("Attack");

        PerformDamageCheck(damage, range);

        nextAttackTime = Time.time + cooldown;
        AttackCooldown = cooldown;

        StartCoroutine(ResetAttack());
        StartCoroutine(ResetIsAttacking());
    }

    // ✅ This method is called from the animation event "DealDamage"
    public void DealDamage()
    {
        Debug.Log("[Weapon] Animation Event Triggered: DealDamage()");

        switch (currentWeapon)
        {
            case 1:
                PerformDamageCheck(melee1Damage, melee1Range);
                break;
            case 2:
                PerformDamageCheck(melee2Damage, melee2Range);
                break;
            case 3:
                PerformDamageCheck(rangedDamage, rangedRange);
                break;
        }
    }

    // 🔍 Enhanced version with debug info for layer and hit detection
    void PerformDamageCheck(int damage, float range)
    {
        Vector3 origin = cam.transform.position + cam.transform.forward * 0.1f;
        float sphereRadius = 0.7f;

        RaycastHit[] hits = Physics.SphereCastAll(origin, sphereRadius, cam.transform.forward, range, dollLayer);

        bool hitSomething = false;

        foreach (var hit in hits)
        {
            string layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);
            Debug.Log($"[Weapon] Hit {hit.collider.name} on layer '{layerName}'");

            DollBehavior doll = hit.collider.GetComponentInParent<DollBehavior>();

            if (doll != null)
            {
                Debug.Log($"[Weapon] Found DollBehavior on {doll.name}. Attempting to apply {damage} damage...");
                doll.TakeDamage(damage);
                Debug.Log($"[Weapon] Damage applied successfully to {doll.name}");
                hitSomething = true;
                break;
            }
            else
            {
                Debug.LogWarning($"[Weapon] Hit {hit.collider.name}, but it has NO DollBehavior script in parent!");
            }
        }

        if (!hitSomething)
        {
            Debug.LogWarning("[Weapon] No valid dolls hit! Check if your dolls are on the correct layer.");
        }

        if (showDebugRay)
        {
            Debug.DrawRay(origin, cam.transform.forward * range, hitSomething ? Color.green : Color.red, 1f);
        }
    }

    IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(AttackCooldown);
        CanAttack = true;
    }

    IEnumerator ResetIsAttacking()
    {
        yield return new WaitForSeconds(HitWindowDuration);
        IsAttacking = false;
    }
}
