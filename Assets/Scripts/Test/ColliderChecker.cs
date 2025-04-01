using UnityEngine;

public class ColliderChecker : MonoBehaviour
{
    // 현재 오브젝트가 다른 콜라이더와 접촉 중이면 true를, 아니면 false를 반환합니다.
    public bool IsColliding()
    {
        // Collider2D 컴포넌트 가져오기
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogWarning("Collider2D 컴포넌트가 없습니다!");
            return false;
        }

        // ContactFilter2D 설정: 트리거 포함
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;

        // "wall" 레이어를 제외한 모든 레이어를 체크하기 위한 레이어 마스크 생성
        int wallLayer = LayerMask.NameToLayer("wall");
        LayerMask mask = ~(1 << wallLayer); // wall 레이어 제외
        filter.SetLayerMask(mask);

        // 검사 결과를 저장할 배열 (최대 10개 정도면 충분합니다)
        Collider2D[] results = new Collider2D[10];
        int count = col.OverlapCollider(filter, results);

        // count가 0보다 크면 현재 접촉 중인 콜라이더가 있는 것으로 판단
        return count > 0;
    }

    public void DebugColliding()
    {
        if (IsColliding()) Debug.Log("게임오버");
        else Debug.Log("성공");
    }
    public void Judge()
    {
        if (IsColliding())
        {
            EventBus.RaiseGameOver();
            Debug.Log("게임오버123");
        }
        else
        {
            EventBus.RaiseSuccess();
            Debug.Log("성공");
        }
    }
}
