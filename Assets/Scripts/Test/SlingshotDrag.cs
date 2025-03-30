using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SlingshotDrag : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 startMousePos;
    private Vector2 endMousePos;
    private bool isDragging = false;

    public float launchPower = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnMouseDown()
    {
        isDragging = true;
        startMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        rb.isKinematic = true; // 드래그 중 움직이지 않게
    }

    void OnMouseDrag()
    {
        // 드래그 중이지만 시각적 피드백이 필요하면 여기에 추가
    }

    void OnMouseUp()
    {
        isDragging = false;
        endMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = startMousePos - endMousePos;
        rb.isKinematic = false;
        rb.AddForce(direction * launchPower, ForceMode2D.Impulse);
    }
}
