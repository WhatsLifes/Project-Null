using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    [Header("Player Settings")]
    public int MaxHealth = 100;
    public int Health = 100;

    [Header("Health Regeneration")]
    public bool enableRegen = true;
    public float regenRate = 20f;
    public float regenDelay = 1f;

    [Header("UI References")]
    public Image bloodyScreenImage;

    private bool isShowingBlood = false;
    private Coroutine bloodCR;
    private Coroutine regenCR;
    private float lastDamageTime;

    private void Start()
    {
        Health = Mathf.Clamp(Health, 0, MaxHealth);
        OnHealthChanged?.Invoke(Health, MaxHealth);

        if (bloodyScreenImage != null)
        {
            Color c = bloodyScreenImage.color;
            c.a = 0f;
            bloodyScreenImage.color = c;
            bloodyScreenImage.enabled = false;
        }

        if (enableRegen)
        {
            regenCR = StartCoroutine(RegenerateHealth());
        }

        StartCoroutine(PostSpawnFix());
    }

    private IEnumerator PostSpawnFix()
    {
        yield return null;

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            cc.enabled = true;
        }
    }

    public void TakeDamage(int damage)
    {
        if (Health <= 0) return;

        Health = Mathf.Clamp(Health - Mathf.Max(0, damage), 0, MaxHealth);
        lastDamageTime = Time.time;

        OnHealthChanged?.Invoke(Health, MaxHealth);

        if (bloodCR != null) StopCoroutine(bloodCR);
        bloodCR = StartCoroutine(BloodyScreenEffect());

        if (Health <= 0)
            PlayerDeath();
    }

    public void Heal(int amount)
    {
        if (Health <= 0) return;

        Health = Mathf.Clamp(Health + Mathf.Max(0, amount), 0, MaxHealth);
        OnHealthChanged?.Invoke(Health, MaxHealth);
    }

    private IEnumerator RegenerateHealth()
    {
        float acc = 0f;

        while (true)
        {
            yield return null;

            if (enableRegen && Health > 0 && Health < MaxHealth)
            {
                if (Time.time - lastDamageTime >= regenDelay)
                {
                    acc += regenRate * Time.deltaTime;

                    if (acc >= 1f)
                    {
                        int hp = Mathf.FloorToInt(acc);
                        acc -= hp;

                        Health = Mathf.Clamp(Health + hp, 0, MaxHealth);
                        OnHealthChanged?.Invoke(Health, MaxHealth);
                    }
                }
                else
                {
                    acc = 0f;
                }
            }
            else
            {
                acc = 0f;
            }
        }
    }

    private void PlayerDeath()
    {
        var fps = GetComponent<SimpleFPS>();
        if (fps != null) fps.enabled = false;

        if (regenCR != null)
        {
            StopCoroutine(regenCR);
            regenCR = null;
        }

        OnDeath?.Invoke();
    }

    private IEnumerator BloodyScreenEffect()
    {
        if (bloodyScreenImage == null) yield break;

        bloodyScreenImage.enabled = true;

        float t = 0f;
        while (t < 0.1f)
        {
            float a = Mathf.Lerp(0f, 1f, t / 0.1f);
            Color c = bloodyScreenImage.color;
            c.a = a;
            bloodyScreenImage.color = c;
            t += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        t = 0f;
        while (t < 1.5f)
        {
            float a = Mathf.Lerp(1f, 0f, t / 1.5f);
            Color c = bloodyScreenImage.color;
            c.a = a;
            bloodyScreenImage.color = c;
            t += Time.deltaTime;
            yield return null;
        }

        bloodyScreenImage.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<TestEnemy>();
            if (enemy != null) TakeDamage(enemy.damage);
        }
    }

    private void OnDestroy()
    {
        if (bloodCR != null) StopCoroutine(bloodCR);
        if (regenCR != null) StopCoroutine(regenCR);
    }
    public void SetRegenerationEnabled(bool enabled)
    {
        enableRegen = enabled;

        if (enabled)
        {
            lastDamageTime = Time.time;
        }
    }
}