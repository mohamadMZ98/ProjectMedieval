using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    public static readonly List<Enemy> ActiveEnemies = new List<Enemy>();

    [Header("Config")]
    [SerializeField] private EnemyData data;

    [Header("Runtime Stats (filled from data)")]
    [SerializeField] private float maxHP = 10f;
    [SerializeField] private float attackDamage = 5f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackInterval = 1.0f;

    private float currentHP;
    private float attackTimer = 0f;
    private SpriteRenderer spriteRenderer;

    #region Static helpers

    public static void DestroyAllActiveEnemies()
    {
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

    #endregion

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (!ActiveEnemies.Contains(this))
        {
            ActiveEnemies.Add(this);
        }
    }

    private void OnDisable()
    {
        ActiveEnemies.Remove(this);
    }

    /// <summary>
    /// Called by WaveSpawner right after Instantiate to choose which enemy type this instance represents.
    /// </summary>
    public void SetData(EnemyData newData)
    {
        data = newData;

        if (data == null)
        {
            Debug.LogWarning($"Enemy {name}: SetData called with null data");
            return;
        }

        maxHP         = data.maxHP;
        attackDamage  = data.attackDamage;
        moveSpeed     = data.moveSpeed;
        attackRange   = data.attackRange;
        attackInterval = data.attackInterval;

        if (spriteRenderer != null && data.sprite != null)
        {
            spriteRenderer.sprite = data.sprite;
        }

        currentHP = maxHP;
    }

    private void Start()
    {
        // If no one called SetData yet but data is assigned in the inspector, use it.
        if (data != null)
        {
            SetData(data);
        }
        else
        {
            currentHP = maxHP;
        }
    }

    private void Update()
    {
        if (RunManager.Instance == null || !RunManager.Instance.IsRunning)
        {
            return;
        }

        MoveTowardsHero();
        HandleAttack();
    }

    private void MoveTowardsHero()
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

    private void HandleAttack()
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

    private void Die()
    {
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
