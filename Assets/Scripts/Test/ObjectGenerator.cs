using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectGenerator : MonoBehaviour
{
    public Vector2 center;
    public float PolygonArea;
    private float dArea = 0.666f;
    private float testPrevArea;

    private GameObject curObject;
    public GameObject prefab;

    public GameObject customObjects;

    private int customSize = 0;
    private int size = 3;
    private int[] counts = { 3, 4, 6 };
    private List<List<Vector2>> paths = new List<List<Vector2>>();
    private Color[] colors = { Color.blue, Color.red, Color.yellow };

    private void Start()
    {
        Cal(PolygonArea);
        CalCustomOutline();
        testPrevArea = PolygonArea;

        if (transform.childCount == 1)
        {
            GenObject();
        }
    }

    void Update()
    {
        if (testPrevArea != PolygonArea)
        {
            testPrevArea = PolygonArea;
            Cal(PolygonArea);
        }
    }

    public void GenObject() //테스트용 퍼블릭
    {
        int idx = Random.Range(0, size+customSize);

        Vector3 position = new Vector3(center.x, center.y, 0f);
        curObject = Instantiate(prefab, position, Quaternion.identity, transform);

        MeshFromCollider meshScript = curObject.GetComponent<MeshFromCollider>();
        meshScript.Init();
        meshScript.collider_.SetPath(0, paths[idx]);
        meshScript.GetMesh();

        MeshRenderer renderer = curObject.GetComponent<MeshRenderer>();
        Material mat = new Material(Shader.Find("Sprites/Default"));

        mat.color = colors[idx%size]; 

        renderer.material = mat;
    }

    void Cal(float p)
    {
        paths.Clear();
        float area = p * dArea;
        for (int i = 0; i < size; i++)
        {
            int n = counts[i];
            paths.Add(new List<Vector2>());

            if (n < 3) n = 3;

            // 한 변의 길이 s 구하기
            float angle = Mathf.PI / n;
            float side = Mathf.Sqrt((4 * area * Mathf.Tan(angle)) / n);

            // 반지름 r 구하기 (중심에서 꼭짓점까지 거리)
            float radius = side / (2 * Mathf.Sin(Mathf.PI / n));

            float initAngle = Random.value * 2 * Mathf.PI;
            // 꼭짓점 좌표 구하기 (시작 각도는 0)
            for (int j = 0; j < n; j++)
            {
                float theta = 2 * Mathf.PI * j / n + initAngle;
                float x = radius * Mathf.Cos(theta);
                float y = radius * Mathf.Sin(theta);
                paths[i].Add(new Vector2(x, y));
            }
        }
    }

    void CalCustomOutline()
    {
        if (customObjects == null) return;

        foreach (Transform obj in customObjects.transform)
        {
            if (obj.GetComponent<PolygonCollider2D>() == null) continue;
            paths.Add(obj.GetComponent<PolygonCollider2D>().GetPath(0).ToList());
            customSize++;
        }
    }
}
