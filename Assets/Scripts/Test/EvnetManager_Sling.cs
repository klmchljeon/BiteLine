using System;
using UnityEngine;

public class EvnetManager_Sling : MonoBehaviour
{
    void Start()
    {
        GameManager.Instance.OnCreated += Creat;
    }

    void OnDestroy()
    {
        GameManager.Instance.OnCreated -= Creat;
    }

    void Creat()
    {
        GetComponent<ObjectGenerator>().GenObject();
    }
}
