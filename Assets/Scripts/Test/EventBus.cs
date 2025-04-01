using System;
using UnityEngine;

public static class EventBus
{
    // === 이벤트 정의 ===

    // 오브젝트가 멈췄다 (예: 모든 자식이 멈춤)
    public static event Action OnObjectStopped;

    // 게임 오버
    public static event Action OnGameOver;

    // 성공 (예: 클리어)
    public static event Action OnSuccess;

    // 자르기 동작이 끝났다
    public static event Action OnCuttingFinished;

    // === 이벤트 발생 함수 ===

    public static void RaiseObjectStopped()
    {
        OnObjectStopped?.Invoke();
    }

    public static void RaiseGameOver()
    {
        OnGameOver?.Invoke();
    }

    public static void RaiseSuccess()
    {
        OnSuccess?.Invoke();
    }

    public static void RaiseCuttingFinished()
    {
        OnCuttingFinished?.Invoke();
    }
}
