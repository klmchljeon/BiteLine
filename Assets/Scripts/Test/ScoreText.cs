using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreText : MonoBehaviour
{
    public TMP_Text curScore;
    public TMP_Text highScore;

    private void Start()
    {
        GetComponent<ScoreManager>().ScoreComplete += ScoreUpdate;
    }

    private void OnDestroy()
    {
        GetComponent<ScoreManager>().ScoreComplete -= ScoreUpdate;
    }

    void ScoreUpdate(int score)
    {
        
    }


}
