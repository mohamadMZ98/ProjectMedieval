using UnityEngine;

public class MapBounds : MonoBehaviour
{
    public static MapBounds Instance { get; private set; }

    [Header("Map size (world units)")]
    public Vector2 size = new Vector2(30f, 30f);

    [Header("Runtime border (optional)")]
    public bool showRuntimeBorder = true;
    public LineRenderer borderLine;

    public Vector2 Center => new Vector2(transform.position.x, transform.position.y);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        SetupRuntimeBorder();
    }

    private void SetupRuntimeBorder()
    {
        if (!showRuntimeBorder || borderLine == null)
            return;

        float halfW = size.x * 0.5f;
        float halfH = size.y * 0.5f;
        Vector3 c = transform.position;

        Vector3 bl = new Vector3(c.x - halfW, c.y - halfH, 0f); // bottom-left
        Vector3 br = new Vector3(c.x + halfW, c.y - halfH, 0f); // bottom-right
        Vector3 tr = new Vector3(c.x + halfW, c.y + halfH, 0f); // top-right
        Vector3 tl = new Vector3(c.x - halfW, c.y + halfH, 0f); // top-left

        borderLine.useWorldSpace = true;
        borderLine.positionCount = 5;
        borderLine.SetPosition(0, bl);
        borderLine.SetPosition(1, br);
        borderLine.SetPosition(2, tr);
        borderLine.SetPosition(3, tl);
        borderLine.SetPosition(4, bl); // close loop
    }

    public Vector3 ClampPosition(Vector3 pos)
    {
        Vector2 center = Center;
        float halfW = size.x * 0.5f;
        float halfH = size.y * 0.5f;

        float x = Mathf.Clamp(pos.x, center.x - halfW, center.x + halfW);
        float y = Mathf.Clamp(pos.y, center.y - halfH, center.y + halfH);

        return new Vector3(x, y, pos.z);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 center = new Vector3(transform.position.x, transform.position.y, 0f);
        Vector3 s = new Vector3(size.x, size.y, 0f);
        Gizmos.DrawWireCube(center, s);
    }
}
