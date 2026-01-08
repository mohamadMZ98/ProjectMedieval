using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ArmyUnit : MonoBehaviour
{
    [SerializeField] private UnitData data;

    private float currentHP;
    private float attackTimer = 0f;

    [Header("Movement")]
    [SerializeField] private float desiredRadius = 1.5f;      // preferred distance from hero
    [SerializeField] private float separationRadius = 0.7f;   // start pushing away when closer than this
    [SerializeField] private float separationStrength = 1.5f; // how strongly they separate

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string isMoving = "isMoving";
    [SerializeField] private string moveX = "moveX";
    [SerializeField] private string moveY = "moveY";

    private Vector2 lastFacing = Vector2.down;

    private static readonly List<ArmyUnit> ActiveUnits = new List<ArmyUnit>();

    private void OnEnable()
    {
        if (!ActiveUnits.Contains(this))
        {
            ActiveUnits.Add(this);
        }
    }

    private void OnDisable()
    {
        ActiveUnits.Remove(this);
    }

    private void Awake()
    {
        var anims = GetComponentsInChildren<Animator>(true);
        Debug.Log($"[{name}] Animators found in children = {anims.Length}", this);

        for (int i = 0; i < anims.Length; i++)
            Debug.Log($"[{name}] Animator[{i}] on '{anims[i].gameObject.name}' controller='" +
                $"{anims[i].runtimeAnimatorController}'", this);
    }

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

    public void Initialize(UnitData unitData)
    {
        data = unitData;
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

        bool unitIsMoving = FollowHeroWithSeparation(hero);
        HandleAutoAttack();
    }

    private bool FollowHeroWithSeparation(Transform hero)
    {
        if (data == null)
            return false;

        Vector3 moveDir = Vector3.zero;

        // 1) Move toward hero until desired radius
        Vector3 toHero = hero.position - transform.position;
        float distToHero = toHero.magnitude;

        if (distToHero > desiredRadius)
        {
            moveDir += toHero.normalized;
        }

        // 2) Separation: push away from nearby units
        for (int i = 0; i < ActiveUnits.Count; i++)
        {
            ArmyUnit other = ActiveUnits[i];
            if (other == null || other == this) continue;

            Vector3 diff = transform.position - other.transform.position;
            float dist = diff.magnitude;

            if (dist > 0f && dist < separationRadius)
            {
                float strength = (separationRadius - dist) / separationRadius;
                moveDir += diff.normalized * strength * separationStrength;
            }
        }

        bool moved = false;

        if (moveDir.sqrMagnitude > 0.0001f)
        {
            moveDir.Normalize();

            Vector3 newPos = transform.position + moveDir * data.moveSpeed * Time.deltaTime;

            if (MapBounds.Instance != null)
            {
                newPos = MapBounds.Instance.ClampPosition(newPos);
            }

            transform.position = newPos;
            moved = true;

            UpdateAnimatorDirection(moveDir);

            UpdateAnimatorMoving(moved);

        }
            return moved;
    }

    private void UpdateAnimatorMoving(bool moving)
    {
        if (animator == null) return;
        animator.SetBool(isMoving, moving);

        // Keep lastFacing when idle — do NOT overwrite moveX/moveY when not moving
        if (!moving)
        {
            animator.SetFloat(moveX, lastFacing.x);
            animator.SetFloat(moveY, lastFacing.y);
        }
    }

    private void UpdateAnimatorDirection(Vector3 moveDir)
    {
        if (animator == null) return;

        Vector2 dir = new Vector2(moveDir.x, moveDir.y);

        // force into Up/Down/Left/Right
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            dir = new Vector2(Mathf.Sign(dir.x), 0f);
        }
        else
        {
            dir = new Vector2(0f, Mathf.Sign(dir.y));
        }

        lastFacing = dir;

        animator.SetFloat(moveX, dir.x);
        animator.SetFloat(moveY, dir.y);
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
