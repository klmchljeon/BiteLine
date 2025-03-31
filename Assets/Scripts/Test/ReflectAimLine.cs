using UnityEngine;

public class ReflectAimLine : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float maxLength = 20f;
    private GameObject layer;

    void Update()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;

        Vector3 origin = transform.position;
        Vector2 dir = (mouseWorld - origin).normalized;

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, dir, maxLength);

        // 필터링: 자기 자신 제외
        RaycastHit2D? validHit = null;
        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject != layer)
            {
                validHit = hit;
                break;
            }
        }

        Vector3[] points = new Vector3[3];
        points[0] = origin;

        if (validHit.HasValue)
        {
            Vector3 hitPoint = validHit.Value.point;
            Vector2 normal = validHit.Value.normal;

            points[1] = hitPoint;

            Vector2 reflectDir = Vector2.Reflect(dir, normal);
            Vector3 secondPoint = hitPoint + (Vector3)(reflectDir * (maxLength - Vector3.Distance(origin, hitPoint)));
            secondPoint.z = 0;
            points[2] = secondPoint;
        }
        else
        {
            // 충돌 없음: 직선
            Vector3 endPoint = origin + (Vector3)(dir * maxLength);
            endPoint.z = 0;
            points[1] = endPoint;
            points[2] = endPoint;
        }

        lineRenderer.positionCount = 3;
        lineRenderer.SetPositions(points);
    }
}
