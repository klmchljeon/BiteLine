using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshFromCollider : MonoBehaviour
{
    void Start()
    {
        var collider = GetComponent<PolygonCollider2D>();
        var meshFilter = GetComponent<MeshFilter>();

        Vector2[] points2D = collider.GetPath(0);

        // 1. Vector2 → Vector3 변환
        Vector3[] vertices = new Vector3[points2D.Length];
        for (int i = 0; i < points2D.Length; i++)
        {
            vertices[i] = points2D[i];
        }

        // 2. 삼각형 분할 (Triangulation)
        Triangulator triangulator = new Triangulator(points2D);
        int[] triangles = triangulator.Triangulate();

        // 3. UV 계산 (간단히 AABB 기준 정규화)
        Vector2[] uvs = new Vector2[points2D.Length];
        Bounds bounds = collider.bounds;
        for (int i = 0; i < points2D.Length; i++)
        {
            Vector2 p = points2D[i] - (Vector2)bounds.min;
            uvs[i] = new Vector2(p.x / bounds.size.x, p.y / bounds.size.y);
        }

        // 4. Mesh 생성 및 할당
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }
}
