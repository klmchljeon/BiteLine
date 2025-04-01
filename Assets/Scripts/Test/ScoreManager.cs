using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int curScore;

    public event Action<int> ScoreComplete;

    private void Start()
    {
        curScore = 0;
        GameManager.Instance.OnSuccess += ScoreUp;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnSuccess -= ScoreUp;
    }

    void ScoreUp()
    {
        curScore++;
        ScoreComplete?.Invoke(curScore);
    }
}
