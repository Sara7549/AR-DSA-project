using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class PointerDrag : MonoBehaviour
{
    [Header("References")]
    public LinkedListGameManager gameManager;
    public Camera arCamera;

    [Header("Settings")]
    public float dragThreshold = 0.02f;

    private Node dragSourceNode = null;
    private LineRenderer dragLine;
    private Vector2 touchStartPos;
    private bool isDragging = false;

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        Touch.onFingerDown += OnFingerDown;
        Touch.onFingerMove += OnFingerMove;
        Touch.onFingerUp += OnFingerUp;
    }

    private void OnDisable()
    {
        Touch.onFingerDown -= OnFingerDown;
        Touch.onFingerMove -= OnFingerMove;
        Touch.onFingerUp -= OnFingerUp;
    }

    private void Start()
    {
        if (arCamera == null)
            arCamera = Camera.main;

        // Create a temporary drag line
        GameObject lineObj = new GameObject("DragLine");
        dragLine = lineObj.AddComponent<LineRenderer>();
        dragLine.positionCount = 2;
        dragLine.startWidth = 0.005f;
        dragLine.endWidth = 0.005f;
        dragLine.material = new Material(
            Shader.Find("Sprites/Default"));
        dragLine.startColor = Color.yellow;
        dragLine.endColor = Color.yellow;
        dragLine.enabled = false;
    }

    private bool isDraggingTemp = false;

    private void OnFingerDown(Finger finger)
    {
        if (finger.index != 0) return;
        touchStartPos = finger.screenPosition;
        isDragging = false;
        isDraggingTemp = false;

        // Check if tapping the temp pointer
        Node tempHit = GetTempPointerAtScreen(finger.screenPosition);
        if (tempHit != null || IsTempPointerTapped(finger.screenPosition))
        {
            isDraggingTemp = true;
            return;
        }

        // Otherwise check if tapping a carriage
        Node tapped = GetNodeAtScreen(finger.screenPosition);
        if (tapped != null)
            dragSourceNode = tapped;
    }

    private void OnFingerMove(Finger finger)
    {
        if (finger.index != 0) return;

        float effectiveDpi = Mathf.Max(Screen.dpi, 160f);
        float dist = Vector2.Distance(
            finger.screenPosition, touchStartPos);
        if (dist > dragThreshold * effectiveDpi)
            isDragging = true;

        if (isDraggingTemp && isDragging)
        {
            // Move temp pointer with finger
            if (TempPointer.Instance != null)
            {
                Vector3 worldPos = ScreenToWorldOnPlane(
                    finger.screenPosition,
                    TempPointer.Instance.transform.position);
                TempPointer.Instance.transform.position =
                    worldPos + Vector3.up * 0.3f;
            }
            return;
        }

        if (dragSourceNode != null && isDragging)
        {
            Vector3 worldPos = ScreenToWorldOnPlane(
                finger.screenPosition,
                dragSourceNode.transform.position);
            dragLine.enabled = true;
            dragLine.SetPosition(0,
                dragSourceNode.transform.position);
            dragLine.SetPosition(1, worldPos);
        }
    }

    private void OnFingerUp(Finger finger)
    {
        if (finger.index != 0) return;
        dragLine.enabled = false;

        if (isDraggingTemp && isDragging)
        {
            // Snap temp to whatever carriage is here
            Node target = GetNodeAtScreen(finger.screenPosition);
            TempPointer.Instance?.PointAt(target);
            gameManager.UpdateReachability();
            isDraggingTemp = false;
            isDragging = false;
            return;
        }

        if (dragSourceNode != null && isDragging)
        {
            // Check if dropped on temp pointer's saved node
            Node target = GetNodeAtScreen(finger.screenPosition);

            if (target != null && target != dragSourceNode)
                dragSourceNode.SetNext(target);
            else if (target == null)
                dragSourceNode.SetNext(null);

            gameManager.UpdateReachability();
        }

        dragSourceNode = null;
        isDragging = false;
        isDraggingTemp = false;
    }

    private bool IsTempPointerTapped(Vector2 screenPos)
    {
        if (TempPointer.Instance == null || arCamera == null)
            return false;

        Vector3 tempWorldPos = TempPointer.Instance.transform.position;
        Vector3 tempScreenPos = arCamera.WorldToScreenPoint(tempWorldPos);

        // If behind camera, ignore
        if (tempScreenPos.z < 0) return false;

        float dist = Vector2.Distance(screenPos,
            new Vector2(tempScreenPos.x, tempScreenPos.y));

       

        return dist < 150f; // increased from 80
    }

    private Node GetTempPointerAtScreen(Vector2 screenPos)
    {
        return null; // handled by IsTempPointerTapped
    }

    private Node GetNodeAtScreen(Vector2 screenPos)
    {
        if (arCamera == null) return null;

        Ray ray = arCamera.ScreenPointToRay(screenPos);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
        System.Array.Sort(hits, (a, b) =>
            a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            Node n = hit.collider.GetComponentInParent<Node>();
            if (n != null) return n;
        }
        return null;
    }

    private Vector3 ScreenToWorldOnPlane(
        Vector2 screenPos, Vector3 planeOrigin)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPos);
        Plane plane = new Plane(Vector3.up, planeOrigin);
        float enter;
        if (plane.Raycast(ray, out enter))
            return ray.GetPoint(enter);
        return planeOrigin;
    }
}