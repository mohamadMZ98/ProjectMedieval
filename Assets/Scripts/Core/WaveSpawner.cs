using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("Enemy Setup")]
    [SerializeField] private Enemy enemyPrefab;      // base prefab (with Enemy + SpriteRenderer)
    [SerializeField] private EnemyData[] enemyTypes; // different enemy types

    [Header("Spawn Timing")]
    [SerializeField] private float initialSpawnInterval = 0.4f;
    [SerializeField] private float minSpawnInterval = 0.3f;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private int maxEnemies = 10;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    private float spawnTimer = 0f;
    private int totalSpawned = 0;

    private void Update()
    {
        if (RunManager.Instance == null || !RunManager.Instance.IsRunning)
            return;

        spawnTimer += Time.deltaTime;

        float progress = RunManager.Instance.RunProgress;
        float currentInterval = Mathf.Lerp(initialSpawnInterval, minSpawnInterval, progress);

        while (spawnTimer >= currentInterval)
        {
            spawnTimer -= currentInterval;
            TrySpawnEnemy(currentInterval);
        }
    }

    private void TrySpawnEnemy(float currentInterval)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("WaveSpawner: enemyPrefab is NULL. Assign a prefab in the inspector.");
            return;
        }

        if (enemyTypes == null || enemyTypes.Length == 0)
        {
            Debug.LogError("WaveSpawner: enemyTypes array is empty. Assign EnemyData assets.");
            return;
        }

        // Cap concurrent enemies
        if (Enemy.ActiveEnemies.Count >= maxEnemies)
        {
            return;
        }

        Transform hero = RunManager.Instance.HeroTransform;
        if (hero == null)
        {
            Debug.LogError("WaveSpawner: HeroTransform is NULL on RunManager.");
            return;
        }

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Vector3 spawnPos = hero.position + new Vector3(randomDir.x, randomDir.y, 0f) * spawnRadius;

        Enemy spawned = Object.Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        // Pick a random enemy type and apply its data & sprite
        EnemyData chosen = enemyTypes[Random.Range(0, enemyTypes.Length)];
        spawned.SetData(chosen);

        totalSpawned++;

        if (enableDebugLogs)
        {
            Debug.Log(
                $"WaveSpawner: Spawned enemy #{totalSpawned} ({chosen.id}) at {spawnPos}. " +
                $"Active = {Enemy.ActiveEnemies.Count}, Max = {maxEnemies}, Interval = {currentInterval:F2}"
            );
        }
    }
}
