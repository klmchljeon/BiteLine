using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextChangeEffect : MonoBehaviour
{
    [SerializeField]
    private GameObject text;

    [SerializeField]
    private string p;

    private int prev = 0;

    void Start()
    {
        GameManager.Instance.OnScoreUp += UpdateText;
        if (p == "high")
        {
            prev = GameManager.Instance.highScore;
            GetComponent<TMP_Text>().text = prev.ToString();
        }
        else
        {
            prev = 0;
            GetComponent<TMP_Text>().text = prev.ToString();
        }
        //prev = (int)GameManager.Instance.gameData[resourceIndex];
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnScoreUp -= UpdateText;
    }

    void UpdateText(string idx)
    {
        if (p != idx) return;

        int newText = idx == "cur" ? GameManager.Instance.curScore : GameManager.Instance.highScore;
        int diff = newText - prev;
        if (diff > 0)
        {
            text.GetComponent<TMP_Text>().text = $"+{diff}";
        }
        else if (diff < 0)
        {
            text.GetComponent<TMP_Text>().text = $"{diff}";
        }
        else
        {
            return;
        }

        GetComponent<TMP_Text>().text = newText.ToString();
        Instantiate(text,transform);
        prev = newText;
    }
}
