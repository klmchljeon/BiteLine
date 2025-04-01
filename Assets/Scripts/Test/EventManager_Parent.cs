using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager_Parent : MonoBehaviour
{
    void Start()
    {
        GameManager.Instance.OnGameOver += Drop;
        GameManager.Instance.OnSuccess += Success;
    }

    void OnDestroy()
    {
        GameManager.Instance.OnGameOver -= Drop;
        GameManager.Instance.OnSuccess -= Success;
    }
    
    void Drop()
    {
        GetComponent<GravityApplier>().ApplyDefaultGravityToChildren();
    }

    void Success()
    {
        GetComponent<PolygonCutter>().Cut(2.5f);
        GetComponent<MoveChildrenUp>().MoveAllChildrenUp();
    }
}
