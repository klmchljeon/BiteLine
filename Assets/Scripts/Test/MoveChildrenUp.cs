using UnityEngine;
using System.Collections;

public class MoveChildrenUp : MonoBehaviour
{
    public float yOffset; // 원하는 만큼의 y 이동값
    public float duration = 1f;       // 얼마만큼의 시간 동안 올릴지

    public void MoveAllChildrenUp()
    {
        StartCoroutine(MoveChildrenCoroutine());
    }

    private IEnumerator MoveChildrenCoroutine()
    {
        float timeElapsed = 0f;

        // 시작 위치 저장
        Transform[] children = new Transform[transform.childCount];
        Vector3[] startPositions = new Vector3[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            children[i] = transform.GetChild(i);
            startPositions[i] = children[i].position;
        }

        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] == null) continue;

                Vector3 targetPosition = startPositions[i] + new Vector3(0, yOffset, 0);
                children[i].position = Vector3.Lerp(startPositions[i], targetPosition, t);
            }

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // 마지막 위치 정확히 지정
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] == null) continue;

            children[i].position = startPositions[i] + new Vector3(0, yOffset, 0);
        }

        EventBus.RaiseCuttingFinished();
    }
}
