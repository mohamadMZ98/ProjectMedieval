using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    public static readonly List<Enemy> ActiveEnemies = new List<Enemy>();

    [SerializeField] private EnemyData data;
    [SerializeField] private float maxHP = 10f;
    [SerializeField] private float attackDamage = 5f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackInterval = 1.0f;

    private float currentHP;
    private float attackTimer = 0f;

    void OnEnable()
    {
        if (!ActiveEnemies.Contains(this))
        {
            ActiveEnemies.Add(this);
        }
    }

    public static void DestroyAllActiveEnemies()
{
    // Destroy all currently alive enemies and clear the list
    for (int i = ActiveEnemies.Count - 1; i >= 0; i--)
    {
        Enemy e = ActiveEnemies[i];
        if (e != null)
        {
            Object.Destroy(e.gameObject);
        }
    }

    ActiveEnemies.Clear();
}


    void OnDisable()
    {
        ActiveEnemies.Remove(this);
    }

    void Start()
    {
        if (data != null)
        {
            maxHP = data.maxHP;
            attackDamage = data.attackDamage;
            moveSpeed = data.moveSpeed;
            attackInterval = data.attackInterval;
        }

        currentHP = maxHP;
    }

    void Update()
    {
        if (RunManager.Instance == null || !RunManager.Instance.IsRunning)
        {
            return;
        }

        MoveTowardsHero();
        HandleAttack();
    }

    void MoveTowardsHero()
    {
        Transform hero = RunManager.Instance.HeroTransform;
        if (hero == null) return;

        Vector3 direction = (hero.position - transform.position).normalized;
        Vector3 newPos = transform.position + direction * moveSpeed * Time.deltaTime;

        if (MapBounds.Instance != null)
        {
            newPos = MapBounds.Instance.ClampPosition(newPos);
        }

        transform.position = newPos;
    }

    void HandleAttack()
    {
        Transform hero = RunManager.Instance.HeroTransform;
        if (hero == null) return;

        float distance = Vector3.Distance(transform.position, hero.position);
        if (distance > attackRange) return;

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            PlayerController heroController = hero.GetComponent<PlayerController>();
            if (heroController != null)
            {
                heroController.ApplyDamage(attackDamage);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        // Make sure this enemy is considered removed BEFORE logging
        ActiveEnemies.Remove(this);

        if (RunManager.Instance != null)
        {
            if (data != null)
            {
                RunManager.Instance.AddHeroXP(data.xpReward);
            }

            RunManager.Instance.OnEnemyKilled(this);
        }

        Destroy(gameObject);
    }
}
