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
    private int bezierSegments = 20;
    private Node snapTarget = null;
    private float snapDistance = 0.15f; // adjust if needed

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

        GameObject lineObj = new GameObject("DragLine");
        dragLine = lineObj.AddComponent<LineRenderer>();
        dragLine.positionCount = bezierSegments + 1; // enough points for curve
        dragLine.startWidth = 0.005f;
        dragLine.endWidth = 0.005f;
        dragLine.material = new Material(Shader.Find("Sprites/Default"));
        dragLine.startColor = Color.yellow;
        dragLine.endColor = Color.yellow;
        dragLine.enabled = false;
    }

    // Add this helper method for Bezier calculation:
    private void DrawBezierLine(Vector3 start, Vector3 end)
    {
        Vector3 startFlat = new Vector3(start.x, start.y + 0.05f, start.z);
        Vector3 endFlat = end;

        float dist = Vector3.Distance(startFlat, endFlat);

        // Always arc upward regardless of direction
        Vector3 mid = (startFlat + endFlat) / 2f + Vector3.up * dist * 0.5f;

        dragLine.positionCount = bezierSegments + 1;
        for (int i = 0; i <= bezierSegments; i++)
        {
            float t = i / (float)bezierSegments;
            Vector3 point = Mathf.Pow(1 - t, 2) * startFlat
                          + 2 * (1 - t) * t * mid
                          + Mathf.Pow(t, 2) * endFlat;
            dragLine.SetPosition(i, point);
        }
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
        float dist = Vector2.Distance(finger.screenPosition, touchStartPos);

        // Use smaller threshold when dragging temp pointer
        float threshold = isDraggingTemp ? dragThreshold * 0.3f : dragThreshold;
        if (dist > threshold * effectiveDpi)
            isDragging = true;


        if (isDraggingTemp && isDragging)
        {
            if (TempPointer.Instance != null)
            {
                TempPointer.Instance.isDragging = true;
                // Calculate where finger is in world space on the same plane as TempPointer
                Vector3 worldPos = ScreenToWorldOnPlane(
                    finger.screenPosition,
                    TempPointer.Instance.transform.position);
                // Tell TempPointer to stretch its arrow to finger, don't move the object
                TempPointer.Instance.UpdateDragArrow(worldPos);
            }
            return;
        }

        if (dragSourceNode != null && isDragging)
        {
            dragSourceNode.SetArrowVisible(true);
            Vector3 worldPos = ScreenToWorldOnPlane(
                finger.screenPosition,
                dragSourceNode.transform.position);

            // Check for nearby snap target
            snapTarget = GetNearestNodeInRange(worldPos);

            if (snapTarget != null)
            {
                // Snap endpoint to target center and turn green
                dragLine.startColor = Color.green;
                dragLine.endColor = Color.green;
                dragLine.enabled = true;
                DrawBezierLine(
                    dragSourceNode.transform.position,
                    snapTarget.transform.position);
            }
            else
            {
                // Normal drag in yellow
                dragLine.startColor = Color.yellow;
                dragLine.endColor = Color.yellow;
                dragLine.enabled = true;
                DrawBezierLine(dragSourceNode.transform.position, worldPos);
            }
        }
    }

    private void OnFingerUp(Finger finger)
    {
        if (finger.index != 0) return;
        dragLine.enabled = false;

        if (isDraggingTemp)
        {
            if (isDragging && TempPointer.Instance != null)
            {
                // Save BEFORE changing anything
                Node previousTempTarget = TempPointer.Instance.pointingAt;

                Node target = GetCarriageAtScreen(finger.screenPosition);
                TempPointer.Instance.EndDrag();
                TempPointer.Instance.PointAt(target);

                gameManager.RecordMove(null, null, previousTempTarget);
                gameManager.UpdateReachability();
            }

            isDraggingTemp = false;
            isDragging = false;
            dragSourceNode = null;
            return;
        }

        if (dragSourceNode != null && isDragging)
        {
            Node target = GetCarriageAtScreen(finger.screenPosition);

            if (target != null && target != dragSourceNode)
            {
                // Save both BEFORE changing anything
                Node prevNext = dragSourceNode.next;
                Node prevTemp = TempPointer.Instance?.pointingAt;

                dragSourceNode.SetNext(target);
                gameManager.RecordMove(dragSourceNode, prevNext, prevTemp);
            }

            dragSourceNode.SetArrowVisible(false);
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



        return dist < 250f; // increased from 80
    }

    private Node GetTempPointerAtScreen(Vector2 screenPos)
    {
        return null; // handled by IsTempPointerTapped
    }

    private Node GetNodeAtScreen(Vector2 screenPos)
    {
        if (arCamera == null) return null;

        Ray ray = arCamera.ScreenPointToRay(screenPos);
        RaycastHit[] hits = Physics.SphereCastAll(ray, 0.05f, 100f);

        System.Array.Sort(hits, (a, b) =>
            a.distance.CompareTo(b.distance));

        Node locomotiveNode = gameManager.GetLocomotive();
        Node fallback = null;

        foreach (RaycastHit hit in hits)
        {
            Node n = hit.collider.GetComponent<Node>()
                   ?? hit.collider.GetComponentInParent<Node>()
                   ?? hit.collider.GetComponentInChildren<Node>();

            if (n == null) continue;

            if (n == locomotiveNode)
            {
                // Only use locomotive as fallback
                fallback = n;
                continue;
            }

            return n; // return first non-locomotive hit
        }

        return fallback; // only return locomotive if nothing else was hit
    }

    // Separate method that excludes locomotive, used only for drop targets
    private Node GetCarriageAtScreen(Vector2 screenPos)
    {
        if (arCamera == null) return null;

        Ray ray = arCamera.ScreenPointToRay(screenPos);
        RaycastHit[] hits = Physics.SphereCastAll(ray, 0.05f, 100f);

        System.Array.Sort(hits, (a, b) =>
            a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            Node n = hit.collider.GetComponent<Node>()
                   ?? hit.collider.GetComponentInParent<Node>()
                   ?? hit.collider.GetComponentInChildren<Node>();
            if (n != null && n != gameManager.GetLocomotive()) return n;
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
    private Node GetNearestNodeInRange(Vector3 worldPos)
    {
        Node nearest = null;
        float nearestDist = snapDistance;

        foreach (Node n in gameManager.GetAllCarriages())
        {
            if (n == dragSourceNode) continue; // skip self
            if (n == gameManager.GetLocomotive()) continue;

            float dist = Vector3.Distance(worldPos, n.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = n;
            }
        }
        return nearest;
    }
}