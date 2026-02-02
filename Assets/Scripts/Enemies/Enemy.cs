using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    public static readonly List<Enemy> ActiveEnemies = new List<Enemy>();

    [Header("Config")]

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Vector3 lastPosition;
    private Vector2 lastMoveDir = Vector2.left;
    private Vector2 lastFacing = Vector2.down;

    [SerializeField] private EnemyData data;

    [Header("Runtime Stats (filled from data)")]
    [SerializeField] private float maxHP = 10f;
    [SerializeField] private float attackDamage = 5f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackInterval = 1.0f;

    [Header("Animator Parameters")]
    [SerializeField] private string isMovingParam = "isMoving";
    [SerializeField] private string moveXParam = "moveX";
    [SerializeField] private string moveYParam = "moveY";
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private string dieTrigger = "Die";

    [SerializeField] private float stopMoveBuffer = 0.05f;

    private bool wantsToMove;
    private float currentHP;
    private float attackTimer = 0f;
    private bool isDead;

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
        if (animator == null) animator = GetComponentInChildren<Animator>(true);
        lastPosition = transform.position;
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
        if (data == null) return;

        // Stats
        maxHP = data.maxHP;
        attackDamage = data.attackDamage;
        moveSpeed = data.moveSpeed;
        attackRange = data.attackRange;
        attackInterval = data.attackInterval;

        // Visuals
        if (spriteRenderer != null && data.sprite != null)
        {
            spriteRenderer.sprite = data.sprite;
        }

        // Animation (optional per enemy type)
        if (animator && data.animatorOverride)
        {
            animator.runtimeAnimatorController = data.animatorOverride;
        }

        currentHP = maxHP;
    }


    private void Start()
    {
        Debug.Log($"Enemy animator obj: {animator.gameObject.name}, controller: {animator.runtimeAnimatorController.name}", this);

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
        if (isDead) return;
        if (RunManager.Instance == null || !RunManager.Instance.IsRunning)
        {
            return;
        }

        MoveTowardsHero();
        HandleAttack();

        

        Vector3 delta3 = transform.position - lastPosition;
        Vector2 delta = new Vector2(delta3.x, delta3.y);

        bool isMoving = delta.sqrMagnitude > 0.0001f;

        if (animator != null)
        {
            animator.SetBool(isMovingParam, wantsToMove);

            if (isMoving)
            {
                // Force to 4 directions (optional, but matches your 4-dir clips)
                Vector2 dir = delta.normalized;

                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                    lastFacing = new Vector2(Mathf.Sign(dir.x), 0f);  // left/right
                else
                    lastFacing = new Vector2(0f, Mathf.Sign(dir.y));  // up/down
            }

            animator.SetFloat(moveXParam, lastFacing.x);
            animator.SetFloat(moveYParam, lastFacing.y);
        }


        lastPosition = transform.position;
    }

    private void MoveTowardsHero()
    {
        Transform hero = RunManager.Instance.HeroTransform;
        if (hero == null) return;

        float dist = Vector3.Distance(transform.position, hero.position);
        if (!wantsToMove)
            wantsToMove = dist > attackRange + stopMoveBuffer;
        else
            wantsToMove = dist > attackRange - stopMoveBuffer;
        if (dist <= attackRange) return;
        if (wantsToMove)
        {
            Vector3 direction = (hero.position - transform.position).normalized;
            Vector3 newPos = transform.position + direction * moveSpeed * Time.deltaTime;

            if (MapBounds.Instance != null)
            {
                newPos = MapBounds.Instance.ClampPosition(newPos);
            }
        transform.position = newPos;
        }
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
            if (animator)
                animator.SetTrigger(attackTrigger);

            PlayerController heroController = hero.GetComponent<PlayerController>();
            if (heroController != null)
            {
                heroController.ApplyDamage(attackDamage);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHP -= damage;

        if (animator)
            animator.SetTrigger(hitTrigger);
            
        
        if (currentHP <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        ActiveEnemies.Remove(this);

        if(animator)
            animator.SetTrigger(dieTrigger);

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
