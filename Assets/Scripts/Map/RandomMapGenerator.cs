using UnityEngine;
using UnityEngine.Tilemaps;

public class RandomMapGenerator : MonoBehaviour
{
    public Tilemap groundTilemap;
    public TileBase[] groundTiles;

    public int width = 50;
    public int height = 50;

    public bool useRandomSeed = true;
    public int seed = 0;

    private System.Random rng;
    private bool hasGenerated = false;

    private void Awake()
    {
        Debug.Log($"[RMG] Awake on instance {GetInstanceID()} at t={Time.time}");
    }

    private void Start()
    {
        Debug.Log($"[RMG] Start on instance {GetInstanceID()} at t={Time.time}");
        GenerateMap();
    }

    [ContextMenu("Generate Map")]
    public void GenerateMap()
    {
        if (hasGenerated && Application.isPlaying)
        {
            Debug.Log($"[RMG] GenerateMap skipped, already generated (t={Time.time})");
            return;
        }

        Debug.Log($"[RMG] GenerateMap RUN (instance {GetInstanceID()}, t={Time.time})");

        if (groundTilemap == null)
        {
            Debug.LogError("RandomMapGenerator: groundTilemap is not assigned.");
            return;
        }

        if (groundTiles == null || groundTiles.Length == 0)
        {
            Debug.LogError("RandomMapGenerator: No groundTiles assigned.");
            return;
        }

        groundTilemap.ClearAllTiles();

        if (useRandomSeed)
            seed = Random.Range(int.MinValue, int.MaxValue);

        rng = new System.Random(seed);

        int halfW = width / 2;
        int halfH = height / 2;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileBase tile = groundTiles[rng.Next(groundTiles.Length)];
                int tileX = x - halfW;
                int tileY = y - halfH;
                groundTilemap.SetTile(new Vector3Int(tileX, tileY, 0), tile);
            }
        }

        hasGenerated = true;
        Debug.Log($"[RMG] DONE map {width}x{height}, seed={seed}, t={Time.time}");
    }
}
