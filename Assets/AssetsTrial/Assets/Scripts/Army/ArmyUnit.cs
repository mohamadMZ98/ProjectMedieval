using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ArmyUnit : MonoBehaviour
{
    [SerializeField] private UnitData data;

    private float currentHP;
    private float attackTimer = 0f;

    // How tightly they hold their own spot
    [SerializeField] private float followDistance = 0.1f;

    // Unique offset around the hero so they do not stack
    private Vector2 formationOffset;

    private void Start()
    {
        if (data != null)
        {
            currentHP = data.maxHP;
        }
        else
        {
            currentHP = 30f; // fallback
            Debug.LogWarning($"ArmyUnit '{name}' has no UnitData assigned. Using fallback stats.");
        }
    }

    public void Initialize(UnitData unitData, Vector2 offset)
    {
        data = unitData;
        formationOffset = offset;

        if (data != null)
        {
            currentHP = data.maxHP;
        }
    }

    private void Update()
    {
        if (RunManager.Instance == null || !RunManager.Instance.IsRunning)
            return;

        Transform hero = RunManager.Instance.HeroTransform;
        if (hero == null)
            return;

        FollowHero(hero);
        HandleAutoAttack();
    }

    private void FollowHero(Transform hero)
    {
        if (data == null)
            return;

        // Each unit has its own “slot” around the hero
        Vector3 targetPos = hero.position + new Vector3(formationOffset.x, formationOffset.y, 0f);
        Vector3 dir = targetPos - transform.position;
        float distance = dir.magnitude;

        if (distance > followDistance)
        {
            dir.Normalize();
            transform.position += dir * data.moveSpeed * Time.deltaTime;
        }
    }

    private void HandleAutoAttack()
    {
        if (data == null)
            return;

        attackTimer += Time.deltaTime;
        if (attackTimer < data.attackInterval)
            return;

        attackTimer = 0f;

        Enemy target = FindNearestEnemyInRange();
        if (target != null)
        {
            target.TakeDamage(data.attackDamage);
        }
    }

    private Enemy FindNearestEnemyInRange()
    {
        if (Enemy.ActiveEnemies.Count == 0)
            return null;

        Enemy nearest = null;
        float nearestDist = float.MaxValue;
        Vector3 pos = transform.position;

        for (int i = 0; i < Enemy.ActiveEnemies.Count; i++)
        {
            Enemy e = Enemy.ActiveEnemies[i];
            if (e == null)
                continue;

            float dist = Vector3.Distance(pos, e.transform.position);
            if (dist < nearestDist && data != null && dist <= data.attackRange)
            {
                nearestDist = dist;
                nearest = e;
            }
        }

        return nearest;
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
