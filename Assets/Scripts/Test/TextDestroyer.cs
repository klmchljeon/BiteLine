using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextDestroyer : MonoBehaviour
{
    public TMP_Text textToBlink;
    public float DestroySpeed = 1.0f; // 사라지는 속도

    public RectTransform textRect;
    public float UpSpeed = 10.0f; // 올라가는 높이

    void Start()
    {
        textToBlink = GetComponent<TMP_Text>();
        textRect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (textToBlink != null)
        {
            Color color = textToBlink.color;
            float alphaChange = DestroySpeed * Time.deltaTime;
            color.a -= alphaChange;

            float rectChange = UpSpeed * Time.deltaTime;
            textRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x, textRect.anchoredPosition.y + rectChange);

            if (color.a <= 0.2f)
            {
                Destroy(gameObject);
            }

            textToBlink.color = color;
        }
    }
}
