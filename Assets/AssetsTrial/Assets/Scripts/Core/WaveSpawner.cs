using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private float initialSpawnInterval = 0.4f;
    [SerializeField] private float minSpawnInterval = 0.3f;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private int maxEnemies = 10;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    private float spawnTimer = 0f;
    private int totalSpawned = 0;

    void Update()
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

    void TrySpawnEnemy(float currentInterval)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("WaveSpawner: enemyPrefab is NULL. Assign a prefab in the inspector.");
            return;
        }

        // Cap concurrent enemies, but no noisy log
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
        totalSpawned++;

        if (enableDebugLogs)
        {
            Debug.Log(
                $"WaveSpawner: Spawned enemy #{totalSpawned} at {spawnPos}. " +
                $"Active = {Enemy.ActiveEnemies.Count}, Max = {maxEnemies}, Interval = {currentInterval:F2}"
            );
        }
    }
}
