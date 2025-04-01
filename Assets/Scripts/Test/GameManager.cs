using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static GameManager Instance { get; private set; }

    public event Action OnEvaluateGameEnd;
    public event Action OnGameOver;
    public event Action OnSuccess;
    public event Action OnCreated;

    void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        // 이벤트 구독
        EventBus.OnObjectStopped += HandleObjectStopped;
        EventBus.OnGameOver += HandleGameOver;
        EventBus.OnSuccess += HandleSuccess;
        EventBus.OnCuttingFinished += HandleCuttingFinished;
    }

    void OnDisable()
    {
        // 이벤트 구독 해제
        EventBus.OnObjectStopped -= HandleObjectStopped;
        EventBus.OnGameOver -= HandleGameOver;
        EventBus.OnSuccess -= HandleSuccess;
        EventBus.OnCuttingFinished -= HandleCuttingFinished;
    }

    // === 이벤트 핸들러 ===

    private void HandleObjectStopped()
    {
        Debug.Log("오브젝트 정지!");
        OnEvaluateGameEnd?.Invoke();
    }

    private void HandleGameOver()
    {
        Debug.Log("게임 오버!");
        OnGameOver?.Invoke();
    }

    private void HandleSuccess()
    {
        Debug.Log("성공!");
        OnSuccess?.Invoke();
    }

    private void HandleCuttingFinished()
    {
        Debug.Log("자르기 완료!");
        OnCreated?.Invoke();
    }
}
