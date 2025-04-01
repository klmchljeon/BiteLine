using UnityEngine;
using System.Collections.Generic;

//[RequireComponent(typeof(Rigidbody2D))]
public class Slingshot : MonoBehaviour
{
    public float UpperBoundY;
    public float launchPower = 10f;

    public Transform ObjectParent;

    private bool isDragging = false;
    private Vector3 dragStartPosition;

    private GameObject slingshotObject;
    private Rigidbody2D rb;
    private SpriteRenderer aimLine;

    private void Start()
    {
        aimLine = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (transform.childCount == 1)
        {
            return;
        }

        // 마우스 클릭 다운
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;

            dragStartPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            slingshotObject = transform.GetChild(1).gameObject;
            rb = slingshotObject.GetComponent<Rigidbody2D>();
            aimLine = transform.GetChild(0).GetComponent<SpriteRenderer>();

            rb.isKinematic = true;
            //Debug.Log("Mouse Down at: " + dragStartPosition);
        }

        // 마우스 클릭 중 드래그
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Debug.Log("Mouse Dragging... Current Position: " + currentMousePosition);

            Debug.Log((currentMousePosition - dragStartPosition).y);
            if ((currentMousePosition - dragStartPosition).y > UpperBoundY)
            {
                aimLine.enabled = false;
                return;
            }

            aimLine.enabled = true;
            Vector3 direction = (currentMousePosition - dragStartPosition);
            float length = direction.magnitude;

            // 방향에 따라 회전
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            aimLine.transform.rotation = Quaternion.Euler(0, 0, angle + 90);

            // 크기 조절 (길이만 늘리고 싶으면 x축만)
            aimLine.transform.localScale = new Vector3(aimLine.transform.localScale.x, length, 1);
            Debug.Log("조준선 나오는 중");

            
        }

        // 마우스 클릭 업
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            Vector3 dragEndPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log((dragEndPosition - dragStartPosition).y);

            rb.isKinematic = false;
            aimLine.enabled = false;

            if ((dragEndPosition-dragStartPosition).y > UpperBoundY)
            {
                return;
            }
            Vector2 direction = dragStartPosition - dragEndPosition;
            rb.AddForce(direction * launchPower, ForceMode2D.Impulse);
            Debug.Log("Mouse Up at: " + dragEndPosition);

            transform.GetChild(1).SetParent(ObjectParent, true);
            ObjectParent.GetComponent<MovementTracker>().ignoreMovementTracking = false;
        }
    }
}
