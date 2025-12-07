using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private HeroStats heroStats = new HeroStats();
    [SerializeField] private float attackInterval = 1.0f;
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float movementSpeedMultiplier = 1.0f;

    private bool canControl = true;
    private float attackTimer = 0f;

    public HeroStats Stats => heroStats;

    void Update()
    {
        if (RunManager.Instance == null || !RunManager.Instance.IsRunning)
        {
            return;
        }

        if (canControl)
        {
            HandleMovement();
        }

        HandleAutoAttack();
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(h, v, 0f).normalized;

        float speed = heroStats.moveSpeed * movementSpeedMultiplier;
        transform.position += dir * speed * Time.deltaTime;
    }

    void HandleAutoAttack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            PerformAutoAttack();
        }
    }

    void PerformAutoAttack()
    {
        if (Enemy.ActiveEnemies.Count == 0)
        {
            return;
        }

        Enemy nearest = null;
        float nearestDist = float.MaxValue;
        Vector3 heroPos = transform.position;

        for (int i = 0; i < Enemy.ActiveEnemies.Count; i++)
        {
            Enemy e = Enemy.ActiveEnemies[i];
            if (e == null) continue;

            float dist = Vector3.Distance(heroPos, e.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = e;
            }
        }

        if (nearest != null && nearestDist <= attackRange)
        {
            nearest.TakeDamage(heroStats.attack);
        }
    }

    public void ApplyDamage(float damage)
    {
        heroStats.TakeDamage(damage);
        if (heroStats.IsDead())
        {
            if (RunManager.Instance != null)
            {
                RunManager.Instance.OnHeroDied();
            }
        }
    }

    public void EnableControl(bool enable)
    {
        canControl = enable;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        LootItem loot = other.GetComponent<LootItem>();
        if (loot != null)
        {
            loot.OnPickedUp(this);
        }
    }
}
