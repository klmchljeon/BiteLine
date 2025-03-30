using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ColliderOutline : MonoBehaviour
{
    public Color outlineColor = Color.white;
    public float width = 0.05f;

    void Start()
    {
        var col = GetComponent<PolygonCollider2D>();
        var line = GetComponent<LineRenderer>();

        Vector2[] points = col.GetPath(0);
        line.positionCount = points.Length + 1;

        for (int i = 0; i < points.Length; i++)
            line.SetPosition(i, points[i]);

        // 마지막 점: 첫 점으로 연결
        line.SetPosition(points.Length, points[0]);

        line.loop = true;
        line.startWidth = line.endWidth = width;
        line.useWorldSpace = false;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = line.endColor = outlineColor;
    }
}
