using UnityEngine;
using System.Collections.Generic;

public class PolygonCutter : MonoBehaviour
{
    /// <summary>
    /// 카메라 기준 수평선 y값을 입력받아 자식 오브젝트들의 폴리곤을 클리핑합니다.
    /// - 수평선보다 완전히 아래: 그대로 유지
    /// - 수평선보다 완전히 위: 오브젝트 삭제
    /// - 교차하는 경우: 수평선 아래 부분만 남김
    /// </summary>
    public void Cut(float lineY)
    {
        // 부모의 모든 자식 오브젝트를 순회합니다.
        foreach (Transform child in transform)
        {
            PolygonCollider2D poly = child.GetComponent<PolygonCollider2D>();
            if (poly == null)
                continue;

            // 1. 자식의 로컬 좌표에 있는 폴리곤 점들을
            //    월드 좌표 → 카메라 좌표로 변환하여 클리핑 연산에 사용합니다.
            Vector2[] localPoints = poly.points;
            List<Vector3> camPoints = new List<Vector3>();
            foreach (Vector2 p in localPoints)
            {
                // 자식 로컬 좌표 → 월드 좌표
                Vector3 worldPoint = poly.transform.TransformPoint(p);
                // 월드 좌표 → 카메라 좌표 (카메라 기준 회전/위치 보정)
                Vector3 camPoint = Camera.main.transform.InverseTransformPoint(worldPoint);
                camPoints.Add(camPoint);
            }

            // 2. 카메라 좌표에서 각 점의 y값을 체크하여, 전체가 below, above, 또는 교차하는지 판단합니다.
            bool hasAbove = false, hasBelow = false;
            foreach (Vector3 cp in camPoints)
            {
                if (cp.y > lineY) hasAbove = true;
                if (cp.y < lineY) hasBelow = true;
            }

            // - 완전히 아래인 경우: (교차 없음, 모든 점이 below) → 아무 처리 없이 유지
            if (!hasAbove)
            {
                continue;
            }
            // - 완전히 위인 경우: (교차 없음, 모든 점이 above) → 오브젝트 삭제
            else if (!hasBelow)
            {
                Destroy(child.gameObject);
                continue;
            }
            // - 교차하는 경우: 클리핑 진행
            else
            {
                List<Vector3> clippedCamPoints = SutherlandHodgmanClip(camPoints, lineY);

                // 클리핑 결과가 3개 미만이면 다각형으로 성립되지 않으므로 삭제
                if (clippedCamPoints.Count < 3)
                {
                    Destroy(child.gameObject);
                    continue;
                }

                // 3. 클리핑된 결과(카메라 좌표)를 다시 월드 좌표 → 자식 로컬 좌표로 변환하여
                //    PolygonCollider2D의 새 점 배열로 할당합니다.
                List<Vector2> newLocalPoints = new List<Vector2>();
                foreach (Vector3 cp in clippedCamPoints)
                {
                    Vector3 worldPoint = Camera.main.transform.TransformPoint(cp);
                    Vector3 localPoint = child.transform.InverseTransformPoint(worldPoint);
                    newLocalPoints.Add(new Vector2(localPoint.x, localPoint.y));
                }

                poly.points = newLocalPoints.ToArray();
                poly.GetComponent<MeshFromCollider>().GetMesh();
            }
        }
    }

    /// <summary>
    /// Sutherland-Hodgman 알고리즘을 사용해, 카메라 좌표상의 다각형을 수평선(clipY) 아래 부분만 남깁니다.
    /// </summary>
    List<Vector3> SutherlandHodgmanClip(List<Vector3> poly, float clipY)
    {
        List<Vector3> output = new List<Vector3>();
        int count = poly.Count;

        // 다각형의 각 에지를 순회 (마지막과 첫 점을 이어줌)
        for (int i = 0; i < count; i++)
        {
            Vector3 current = poly[i];
            Vector3 next = poly[(i + 1) % count];

            // inside 조건: y가 clipY 이하 (즉, 수평선 아래)
            bool currentInside = current.y <= clipY;
            bool nextInside = next.y <= clipY;

            if (currentInside && nextInside)
            {
                // 둘 다 내부: 다음 점을 추가
                output.Add(next);
            }
            else if (currentInside && !nextInside)
            {
                // 내부에서 외부로 나가는 경우: 교차점만 추가
                Vector3 intersection = ComputeIntersection(current, next, clipY);
                output.Add(intersection);
            }
            else if (!currentInside && nextInside)
            {
                // 외부에서 내부로 들어오는 경우: 교차점과 다음 점을 추가
                Vector3 intersection = ComputeIntersection(current, next, clipY);
                output.Add(intersection);
                output.Add(next);
            }
            // 두 점 모두 외부인 경우는 아무것도 추가하지 않음
        }
        return output;
    }

    /// <summary>
    /// 두 점 (p1, p2) 사이에서 수평선 y = clipY와의 교차점을 계산합니다.
    /// </summary>
    Vector3 ComputeIntersection(Vector3 p1, Vector3 p2, float clipY)
    {
        // 선형 보간법으로 t 값을 구함 (p2.y - p1.y 가 0가 아닌 상황)
        float t = (clipY - p1.y) / (p2.y - p1.y);
        return p1 + t * (p2 - p1);
    }
}
