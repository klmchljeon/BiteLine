using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static GameManager Instance { get; private set; }

    public event Action OnEvaluateGameEnd;
    public event Action OnGameOver;
    public event Action OnSuccess;
    public event Action<string> OnScoreUp;
    public event Action OnCreated;

    public int curScore;
    public int highScore;
    const string ScoreKey = "HighScore";

    void Awake()
    {
        curScore = 0;
        // 싱글톤 패턴 구현
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        highScore = PlayerPrefs.GetInt(ScoreKey,0);

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

    private void Start()
    {
        curScore = 0;
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
        if (highScore < curScore)
        {
            highScore = curScore;
            PlayerPrefs.SetInt(ScoreKey, highScore);
            OnScoreUp?.Invoke("high");
        }

        OnGameOver?.Invoke();
    }

    private void HandleSuccess()
    {
        Debug.Log("성공!");
        curScore++;

        OnScoreUp?.Invoke("cur");
        OnSuccess?.Invoke();
    }

    private void HandleCuttingFinished()
    {
        Debug.Log("자르기 완료!");
        OnCreated?.Invoke();
    }
}
