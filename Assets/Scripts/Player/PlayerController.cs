using System.Diagnostics;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private HeroStats heroStats = new HeroStats();
    [SerializeField] private float attackInterval = 1.0f;
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float movementSpeedMultiplier = 1.0f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visualRoot;
    private Vector2 lastMoveDir = Vector2.left;

    [Header("Animator Param Names")]
    //[SerializeField] private string speedParam = "Speed";         // float
    [SerializeField] private string isMovingParam = "1_Move";   // bool (optional)
    //[SerializeField] private string moveXParam = "moveX";         // float (optional)
    //[SerializeField] private string moveYParam = "moveY";         // float (optional)
    [SerializeField] private string attackTrigger = "2_Attack";     // trigger
    [SerializeField] private string hitTrigger = "3_Damage";           // trigger
    [SerializeField] private string dieTrigger = "4_Death";           // trigger
    [SerializeField] private string deadBool = "IsDeath";          // bool (optional)

    private bool canControl = true;
    private float attackTimer = 0f;

    public HeroStats Stats => heroStats;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);

        if (visualRoot == null)
            visualRoot = transform;
    }

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

    private void UpdateFacing(Vector2 dir)
    {
        if (visualRoot == null) return;

        // Only flip when moving left/right
        if (Mathf.Abs(dir.x) < 0.01f) return;

        Vector3 s = visualRoot.localScale;

        
        s.x = Mathf.Abs(s.x) * (dir.x > 0f ? -1f : 1f);

        visualRoot.localScale = s;
    }

    void HandleMovement()
    {
        float h = UnityEngine.Input.GetAxisRaw("Horizontal");
        float v = UnityEngine.Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(h, v, 0f).normalized;
        bool moving = dir.sqrMagnitude > 0.01f;

        if (moving)
        {
            lastMoveDir = dir;
            UpdateFacing(lastMoveDir);
        }



        float speed = heroStats.moveSpeed * movementSpeedMultiplier;
        Vector3 newPos = transform.position + dir * speed * Time.deltaTime;

        // Clamp to map bounds if present
        if (MapBounds.Instance != null)
        {
            newPos = MapBounds.Instance.ClampPosition(newPos);
        }

        transform.position = newPos;

        if (animator != null)
        {
            animator.SetBool(isMovingParam, moving);
        }
        
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
        if (!heroStats.IsDead())
            animator.SetTrigger(hitTrigger);

        heroStats.TakeDamage(damage);

        if (heroStats.IsDead())
        {
            canControl = false;
            animator.SetBool(deadBool, true);
            animator.SetTrigger(dieTrigger);

            RunManager.Instance?.OnHeroDied();
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

    private void TriggerAnim(string triggerName)
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(triggerName)) return;

        // Only triggers if parameter exists in controller (prevents silent confusion)
        animator.SetTrigger(triggerName);
    }

    private void SetBoolAnim(string boolName, bool value)
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(boolName)) return;

        animator.SetBool(boolName, value);
    }
}
