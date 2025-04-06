using UnityEngine;
using System.Collections.Generic;
using System;

public class PolygonCutter : MonoBehaviour
{
    class IntersectionRecord
    {
        public Vector3 point;
        public int boundaryID;    // 이후 경계선(경계 조각) 식별에 사용
        public bool isEntry;      // true: 외부에서 내부(enter), false: 내부에서 외부(exit)
    }

    // 한 경계선(클리핑 선과의 교차를 이루는 하나의 연속 구간)을 표현하는 자료구조
    class Boundary
    {
        public IntersectionRecord entryIntersection;  // 외부→내부 교차점
        public IntersectionRecord exitIntersection;   // 내부→외부 교차점
        public List<Vector3> middlePoints;
        public bool processed = false;  // 케이스 6.2 처리 후, 재사용 방지를 위한 플래그
    }

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
                List<List<Vector3>> clippedCamPointsList = SutherlandHodgmanClipList(camPoints, lineY);

                foreach (List<Vector3> clippedCamPoints in clippedCamPointsList)
                {
                    List<Vector2> newLocalPoints = new List<Vector2>();
                    foreach (Vector3 cp in clippedCamPoints)
                    {
                        Vector3 worldPoint = Camera.main.transform.TransformPoint(cp);
                        Vector3 localPoint = child.transform.InverseTransformPoint(worldPoint);
                        newLocalPoints.Add(new Vector2(localPoint.x, localPoint.y));
                    }

                    GenPiece(child.gameObject, newLocalPoints);
                }

                //poly.points = newLocalPoints.ToArray();
                //poly.GetComponent<MeshFromCollider>().GetMesh();
                Destroy(child.gameObject);

                Debug.Log($"현재 클리핑된 조각 개수: {clippedCamPointsList.Count}");
            }
        }
    }

    void GenPiece(GameObject obj, List<Vector2> poly)
    {
        GameObject clone = Instantiate(obj, transform);
        
        PolygonCollider2D polygon = clone.GetComponent<PolygonCollider2D>();
        polygon.enabled = true;
        polygon.points = poly.ToArray();

        MeshFromCollider mesh = clone.GetComponent<MeshFromCollider>();
        mesh.enabled = true;
        mesh.Init();
        mesh.GetMesh();

        return;
    }

    List<List<Vector3>> SutherlandHodgmanClipList(List<Vector3> poly, float clipY)
    {
        int n = poly.Count;
        if (n < 3)
            return new List<List<Vector3>>();

        // 1. 선분 순회 및 교차점(경계선) 추출
        List<Boundary> boundaries = new List<Boundary>();

        // (1) 최초로 "외부→내부" 교차가 발생하는 선분 인덱스 찾기
        int startIndex = -1;
        for (int i = 0; i < n; i++)
        {
            int next = (i + 1) % n;
            bool currentInside = (poly[i].y <= clipY);
            bool nextInside = (poly[next].y <= clipY);
            if (!currentInside && nextInside)
            {
                startIndex = i;
                break;
            }
        }
        if (startIndex == -1)
        {
            // 교차점이 반드시 발생함이 보장되므로 여기까지 오면 문제가 있음.
            return new List<List<Vector3>>();
            //throw new Exception("교차점이 존재해야 하는데 발견되지 않았습니다.");
        }

        // (2) 시작 인덱스부터 원형 순회하면서 각 교차(경계선)를 기록
        for (int i = startIndex; i < startIndex + n; i++)
        {
            int index = i % n;
            int next = (index + 1) % n;
            bool currentInside = (poly[index].y <= clipY);
            bool nextInside = (poly[next].y <= clipY);

            if (currentInside != nextInside) // 한쪽은 내부, 한쪽은 외부이면 교차 발생
            {
                Vector3 intersect = ComputeIntersection(poly[index], poly[next], clipY);

                if (!currentInside && nextInside)
                {
                    // 외부에서 내부로 들어오는 경우 → 새로운 경계선 시작
                    Boundary boundary = new Boundary();
                    IntersectionRecord entry = new IntersectionRecord()
                    {
                        point = intersect,
                        isEntry = true
                    };
                    boundary.entryIntersection = entry;
                    boundary.middlePoints = new List<Vector3>() { poly[next] };

                    boundaries.Add(boundary);
                }
                else if (currentInside && !nextInside)
                {
                    // 내부에서 외부로 나가는 경우 → 가장 최근에 시작한 경계선의 종료점 지정
                    if (boundaries.Count == 0)
                        throw new Exception("내부→외부 교차가 있으나 대응하는 경계선이 없습니다.");

                    Boundary boundary = boundaries[boundaries.Count - 1];
                    IntersectionRecord exit = new IntersectionRecord()
                    {
                        point = intersect,
                        isEntry = false
                    };
                    boundary.exitIntersection = exit;
                }
            }
            else
            {
                if (!currentInside) continue;

                //둘다 내부라면 가장 최근 경계선에 다음 점 추가
                boundaries[boundaries.Count - 1].middlePoints.Add(poly[next]);
            }
        }

        int boundaryID = 0;
        List<IntersectionRecord> allIntersections = new List<IntersectionRecord>();
        foreach (var boundary in boundaries)
        {
            // entry와 exit 둘 다 있어야 함 (보장됨)
            boundary.entryIntersection.boundaryID = boundaryID;
            boundary.exitIntersection.boundaryID = boundaryID;
            allIntersections.Add(boundary.entryIntersection);
            allIntersections.Add(boundary.exitIntersection);
            boundaryID++;
        }

        // 3. 모든 교차점을 x좌표 기준으로 정렬 (y는 clipY로 동일)
        allIntersections.Sort((a, b) => a.point.x.CompareTo(b.point.x));

        // 4. 정렬된 교차점을 2개씩 묶어 페어 생성
        List<Tuple<IntersectionRecord, IntersectionRecord>> pairs = new List<Tuple<IntersectionRecord, IntersectionRecord>>();
        for (int i = 0; i < allIntersections.Count; i += 2)
        {
            pairs.Add(new Tuple<IntersectionRecord, IntersectionRecord>(allIntersections[i], allIntersections[i + 1]));
        }
        
        HashSet<int> visited = new HashSet<int>();

        // 5. 페어별로 내부 영역(다각형 조각)을 구성
        List<List<Vector3>> resultPolygons = new List<List<Vector3>>();
        foreach (var pair in pairs)
        {
            IntersectionRecord a = pair.Item1;
            IntersectionRecord b = pair.Item2;
            if (visited.Contains(a.boundaryID)) continue;

            // (6.1) 두 교차점이 동일한 경계선에 속하는 경우: 해당 경계선 구간 그대로 사용
            if (a.boundaryID == b.boundaryID)
            {
                List<Vector3> polyPiece = GetPolygonPiece(a.boundaryID, boundaries);
                resultPolygons.Add(polyPiece);
            }
            else
            {
                // (6.2) 두 교차점이 다른 경계선에 속하는 경우:
                // a의 "다른" 교차점와 b의 "다른" 교차점을 이용해 올바른 순서로 다각형 조각 구성
                List<Vector3> polyPiece = BuildComplexPolygonPiece(a.boundaryID, b.boundaryID, boundaries);
                resultPolygons.Add(polyPiece);

                visited.Add(a.boundaryID);
                visited.Add(b.boundaryID);
            }
        }

        return resultPolygons;
    }

    List<Vector3> GetPolygonPiece(int a, List<Boundary> boundaries)
    {
        // 이 함수는 a에서 b까지, 다각형 상의 내부(clipY 이하) 영역을 따라 순회하며 점들을 수집한다고 가정합니다.
        // 실제 구현에서는 a와 b 사이의 poly의 인접한 점들을 추출하고, a, b의 교차점도 포함해야 합니다.
        List<Vector3> piece = new List<Vector3>();
        Boundary k = new Boundary();
        foreach (Boundary b in boundaries)
        {
            if (b.entryIntersection.boundaryID == a)
            {
                k = b; break;
            }
        }

        if (k == null) return new List<Vector3>();

        piece.Add(k.entryIntersection.point);
        for (int i = 0; i < k.middlePoints.Count; i++)
        {
            piece.Add(k.middlePoints[i]);
        }
        piece.Add(k.exitIntersection.point);

        return piece;
    }

    List<Vector3> BuildComplexPolygonPiece(int a, int b, List<Boundary> boundaries)
    {
        // 이 함수는 다음 순서로 조각을 구성합니다.
        // a -> (a의 중간 점들) -> a의 끝 -> b의 끝 -> (b의 중간 점들) -> b
        List<Vector3> piece = new List<Vector3>();
        Boundary bd1 = new Boundary();
        Boundary bd2 = new Boundary();
        foreach (Boundary tmp in boundaries)
        {
            if (tmp.entryIntersection.boundaryID == a)
            {
                bd1 = tmp;
            }

            if (tmp.entryIntersection.boundaryID == b)
            {
                bd2 = tmp;
            }
        }

        if (bd1 == null || bd2 == null) return new List<Vector3>();

        piece.Add(bd1.entryIntersection.point);
        for (int i = 0; i < bd1.middlePoints.Count; i++)
        {
            piece.Add(bd1.middlePoints[i]);
        }
        piece.Add(bd1.exitIntersection.point);

        piece.Add(bd2.entryIntersection.point);
        for (int i = 0; i < bd2.middlePoints.Count; i++)
        {
            piece.Add(bd2.middlePoints[i]);
        }
        piece.Add(bd2.exitIntersection.point);

        return piece;
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
        if (p2.y - p1.y == 0) p1.y += 0.0001f;
        // 선형 보간법으로 t 값을 구함 (p2.y - p1.y 가 0가 아닌 상황)
        float t = (clipY - p1.y) / (p2.y - p1.y);
        return p1 + t * (p2 - p1);
    }
}
