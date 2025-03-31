using UnityEngine;

public class HorizontalLineTest : MonoBehaviour
{
    // 기준 y값 (카메라 좌표계 기준)
    public float lineY = 0f;
    public Color lineColor = Color.red;

    void Update()
    {
        DrawHorizontalLine();
        //CheckChildPolygons();
    }

    // 카메라의 시점을 기준으로, 객체들이 위치한 z값에 맞게 수평선을 그립니다.
    void DrawHorizontalLine()
    {
        // 2D 환경에서 객체들은 일반적으로 z=0에 위치합니다.
        // 카메라의 로컬 좌표계에서, world의 (0,0,0)이 어느 z에 위치하는지 계산합니다.
        Vector3 objectPosInCameraSpace = Camera.main.transform.InverseTransformPoint(Vector3.zero);
        float objectZInCameraSpace = objectPosInCameraSpace.z;

        // 카메라 로컬 좌표계에서 x좌표를 넓게 잡아 y=lineY인 두 점을 생성합니다.
        Vector3 camPointLeft = new Vector3(-1000f, lineY, objectZInCameraSpace);
        Vector3 camPointRight = new Vector3(1000f, lineY, objectZInCameraSpace);

        // 이 점들을 world 좌표로 변환하여 디버그 선으로 그림
        Vector3 worldPointLeft = Camera.main.transform.TransformPoint(camPointLeft);
        Vector3 worldPointRight = Camera.main.transform.TransformPoint(camPointRight);

        Debug.DrawLine(worldPointLeft, worldPointRight, lineColor);
    }

    // 부모의 모든 자식에 대해 PolygonCollider2D를 찾아 수평선과의 상대 위치(아래, 위, 교차)를 체크합니다.
    void CheckChildPolygons()
    {
        foreach (Transform child in transform)
        {
            PolygonCollider2D poly = child.GetComponent<PolygonCollider2D>();
            if (poly == null)
                continue;

            PolygonStatus status = CheckPolygon(poly, lineY);
            switch (status)
            {
                case PolygonStatus.EntirelyBelow:
                    Debug.Log(child.name + " is entirely below the line.");
                    break;
                case PolygonStatus.EntirelyAbove:
                    Debug.Log(child.name + " is entirely above the line.");
                    break;
                case PolygonStatus.Intersecting:
                    Debug.Log(child.name + " intersects the line.");
                    break;
            }
        }
    }

    // 각 다각형의 상태를 나타내는 열거형
    enum PolygonStatus { EntirelyBelow, EntirelyAbove, Intersecting };

    // PolygonCollider2D의 점들을 자식의 Transform을 통해 world 좌표로 변환한 뒤,
    // 카메라 좌표로 보정하여 y값을 비교합니다.
    PolygonStatus CheckPolygon(PolygonCollider2D poly, float yValue)
    {
        Vector2[] localPoints = poly.points;
        bool foundAbove = false;
        bool foundBelow = false;

        foreach (Vector2 localPoint in localPoints)
        {
            // 자식 로컬 좌표 -> world 좌표 변환
            Vector3 worldPoint = poly.transform.TransformPoint(localPoint);
            // world 좌표 -> 카메라 좌표 (카메라 기준으로 회전, 위치 보정)
            Vector3 camPoint = Camera.main.transform.InverseTransformPoint(worldPoint);

            if (camPoint.y > yValue)
                foundAbove = true;
            if (camPoint.y < yValue)
                foundBelow = true;
        }

        // 모두 y값 이하이면 완전히 아래, 모두 y값 이상이면 완전히 위,
        // 그 외에는 교차하는 것으로 판단합니다.
        if (!foundAbove)
            return PolygonStatus.EntirelyBelow;
        else if (!foundBelow)
            return PolygonStatus.EntirelyAbove;
        else
            return PolygonStatus.Intersecting;
    }
}
