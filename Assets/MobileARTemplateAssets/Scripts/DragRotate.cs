using UnityEngine;
using UnityEngine;
using UnityEngine.InputSystem;

public class DragRotate : MonoBehaviour
{
    [Header("Hýz Ayarlarý")]
    public float rotationSpeed = 0.2f;
    public float panSpeed = 0.005f;
    public float zoomSpeed = 0.01f;

    private Vector2 prevTouchPos;
    private bool isDragging = false;

    private Vector2 prevTwoFingerAvg;
    private float prevTouchDistance = -1f;
    private bool isZooming = false;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    public void ResetTransform()
    {
        transform.position = new Vector3(0f, 0f, 10f); // x = 0, y = 0, z = 10
        transform.rotation = Quaternion.identity;      // rotasyon sýfýrlanýr (Euler 0,0,0)
    }


    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseRotation();
#else
        if (Touchscreen.current == null) return;

        int activeTouches = 0;
        foreach (var t in Touchscreen.current.touches)
            if (t.press.isPressed) activeTouches++;

        if (activeTouches == 1)
        {
            isZooming = false;
            HandleSingleTouchRotation();
        }
        else if (activeTouches >= 2)
        {
            HandleTwoFingerPanAndZoom();
        }
        else
        {
            isDragging = false;
            isZooming = false;
            prevTouchDistance = -1f;
        }
#endif
    }

    // -------------------- ROTATION (PC) --------------------
    void HandleMouseRotation()
    {
        if (!Mouse.current.leftButton.isPressed)
        {
            isDragging = false;
            return;
        }

        Vector2 curPos = Mouse.current.position.ReadValue();

        if (!isDragging)
        {
            isDragging = true;
            prevTouchPos = curPos;
            return;
        }

        Vector2 delta = curPos - prevTouchPos;
        prevTouchPos = curPos;

        RotateXYLocal(delta);
    }

    // -------------------- ROTATION (TOUCH) --------------------
    void HandleSingleTouchRotation()
    {
        var touch = Touchscreen.current.primaryTouch;

        if (!touch.press.isPressed)
        {
            isDragging = false;
            return;
        }

        Vector2 curPos = touch.position.ReadValue();

        if (!isDragging)
        {
            isDragging = true;
            prevTouchPos = curPos;
            return;
        }

        Vector2 delta = curPos - prevTouchPos;
        prevTouchPos = curPos;

        RotateXYLocal(delta);
    }

    // -------------------- PAN + ZOOM (2 parmak) --------------------
    void HandleTwoFingerPanAndZoom()
    {
        Vector2 p1 = Vector2.zero, p2 = Vector2.zero;
        int count = 0;

        foreach (var t in Touchscreen.current.touches)
        {
            if (t.press.isPressed)
            {
                if (count == 0) p1 = t.position.ReadValue();
                else if (count == 1) p2 = t.position.ReadValue();
                count++;
                if (count == 2) break;
            }
        }

        if (count < 2) return;

        Vector2 avg = (p1 + p2) / 2f;
        float dist = Vector2.Distance(p1, p2);

        if (!isZooming || prevTouchDistance < 0f)
        {
            isZooming = true;
            prevTwoFingerAvg = avg;
            prevTouchDistance = dist;
            return;
        }

        Vector2 deltaPan = avg - prevTwoFingerAvg;
        float deltaZoom = dist - prevTouchDistance;

        prevTwoFingerAvg = avg;
        prevTouchDistance = dist;

        //  PAN (sað–sol ve yukarý–aþaðý)
        Vector3 pan = new Vector3(deltaPan.x * panSpeed, deltaPan.y * panSpeed, 0f);
        transform.position += pan;

        //  ZOOM (kamera yönünde)
        Vector3 zoomDir = -Camera.main.transform.forward;
        transform.position += zoomDir * (deltaZoom * zoomSpeed);
    }



    // -------------------- LOCAL (Pivot) Y & X Rotate --------------------
    void RotateXYLocal(Vector2 delta)
    {
        // Y ekseninde sað-sol
        transform.Rotate(Vector3.up, -delta.x * rotationSpeed, Space.Self);

        // X ekseninde yukarý-aþaðý
        transform.Rotate(Vector3.right, delta.y * rotationSpeed, Space.Self);
    }
}
