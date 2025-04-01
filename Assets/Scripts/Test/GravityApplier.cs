using UnityEngine;

public class GravityApplier : MonoBehaviour
{
    // 기본 gravityScale 값 (필요에 따라 인스펙터에서 조절 가능)
    public float defaultGravityScale = 1f;

    // 모든 자식 오브젝트의 Rigidbody2D 컴포넌트에 기본 gravity 값을 설정합니다.
    public void ApplyDefaultGravityToChildren()
    {
        // 현재 오브젝트의 모든 자식 오브젝트를 순회
        foreach (Transform child in transform)
        {
            // 자식 오브젝트에서 Rigidbody2D 컴포넌트 가져오기
            Rigidbody2D rb = child.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Rigidbody2D의 gravityScale을 기본값으로 설정
                rb.gravityScale = defaultGravityScale;
            }
        }
    }
}
