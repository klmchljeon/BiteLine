using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager_Collider : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.OnEvaluateGameEnd += Judge;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnEvaluateGameEnd -= Judge;
    }

    void Judge()
    {
        GetComponent<ColliderChecker>().Judge();
    }
}
