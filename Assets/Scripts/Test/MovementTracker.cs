using UnityEngine;
using System.Collections.Generic;

public class MovementTracker : MonoBehaviour
{
    // 외부 이벤트에 의해 제어될 플래그 (true일 경우 FixedUpdate를 일찍 리턴)
    public bool ignoreMovementTracking = true;

    public float velocityThreshold = 0.1f; // 움직임 판단 임계값
    public float windowDuration = 0.5f;    // 슬라이딩 윈도우 시간 (초)

    // (타임스탬프, 상태)를 저장하는 구조체 (상태: 1은 움직임 있음, 0은 없음)
    private struct MovementState
    {
        public float timestamp;
        public int state;

        public MovementState(float time, int s)
        {
            timestamp = time;
            state = s;
        }
    }

    // 슬라이딩 윈도우 데이터를 저장할 큐
    private Queue<MovementState> movementQueue = new Queue<MovementState>();
    // 현재 슬라이딩 윈도우 내에 움직임이 있었던 프레임(1)의 누적 합
    private int cnt = 0;
    // 최초로 충분한 시간이 쌓여 0.5초 이상이 되었음을 체크하는 플래그
    private bool windowFilled = false;

    void FixedUpdate()
    {
        // 외부 플래그가 true면, 움직임 판단 코드를 수행하지 않음.
        if (ignoreMovementTracking)
            return;

        float currentTime = Time.time;

        // 각 프레임의 움직임 상태 결정: 하나라도 임계값을 초과하는 자식이 있다면 1, 없으면 0
        int currentState = 0;
        foreach (Transform child in transform)
        {
            Rigidbody2D rb = child.GetComponent<Rigidbody2D>();
            if (rb != null && rb.velocity.magnitude > velocityThreshold)
            {
                currentState = 1;
                break; // 하나라도 움직이면 바로 1로 판정
            }
        }

        // 현재 시간과 상태를 큐에 저장하고, 상태가 1이면 카운트 증가
        movementQueue.Enqueue(new MovementState(currentTime, currentState));
        if (currentState == 1)
            cnt++;

        // 큐의 맨 앞 항목이 windowDuration(0.5초)를 초과하면 제거하며 카운트 갱신
        while (movementQueue.Count > 0 && (currentTime - movementQueue.Peek().timestamp) > windowDuration)
        {
            MovementState oldState = movementQueue.Dequeue();
            if (oldState.state == 1)
                cnt--;

            windowFilled = true; // 최초 dequeue가 일어난 이후부터 슬라이딩 윈도우가 유효
        }

        // 슬라이딩 윈도우가 꽉 찬 상태에서 cnt가 0이면, 지난 0.5초 동안 자식 오브젝트가 전부 정지한 상태임
        if (windowFilled && cnt == 0)
        {
            HandleNoMovementDetected();
        }
    }

    // 모든 자식이 움직이지 않았을 때 호출되는 함수 (디버그 로그 출력 및 초기화)
    private void HandleNoMovementDetected()
    {
        Debug.Log("지난 0.5초 동안 모든 자식 오브젝트가 정지 상태입니다.");
        ResetTracking();
    }

    // 큐, 카운터, 슬라이딩 윈도우 관련 상태 초기화
    private void ResetTracking()
    {
        ignoreMovementTracking = true;
        movementQueue.Clear();
        cnt = 0;
        windowFilled = false;
    }
}
