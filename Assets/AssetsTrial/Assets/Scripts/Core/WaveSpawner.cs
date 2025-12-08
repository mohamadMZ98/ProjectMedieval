using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private float initialSpawnInterval = 2f;
    [SerializeField] private float minSpawnInterval = 0.3f;
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private int maxEnemies = 200;

    private float spawnTimer = 0f;

    void Update()
    {
        if (RunManager.Instance == null || !RunManager.Instance.IsRunning)
            return;

        spawnTimer += Time.deltaTime;

        float progress = RunManager.Instance.RunProgress;
        float currentInterval = Mathf.Lerp(initialSpawnInterval, minSpawnInterval, progress);

        if (spawnTimer >= currentInterval)
        {
            spawnTimer = 0f;
            TrySpawnEnemy();
        }
    }

    void TrySpawnEnemy()
    {
        if (enemyPrefab == null)
            return;

        if (Enemy.ActiveEnemies.Count >= maxEnemies)
            return;

        Transform hero = RunManager.Instance.HeroTransform;
        if (hero == null)
            return;

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Vector3 spawnPos = hero.position + new Vector3(randomDir.x, randomDir.y, 0f) * spawnRadius;

        Enemy spawned = Object.Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        Debug.Log($"Spawned enemy at {spawnPos}. Active = {Enemy.ActiveEnemies.Count}");
    }
}
