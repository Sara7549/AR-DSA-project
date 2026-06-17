using UnityEngine;
using TMPro;

public class TempPointer : MonoBehaviour
{
    public static TempPointer Instance;

    [Header("References")]
    public TextMeshProUGUI label;

    [Header("Settings")]
    public float floatHeight = 0.5f;
    public float moveSpeed = 8f;

    public Node pointingAt { get; private set; }
    public bool isDragging = false;

    private LineRenderer shaft;
    private LineRenderer arrowLeft;
    private LineRenderer arrowRight;

    // Separate drag line that stretches to finger
    private LineRenderer dragShaft;
    private LineRenderer dragArrowLeft;
    private LineRenderer dragArrowRight;

    private Vector3 targetPosition;
    private bool isPlaced = false;

    // Where the finger currently is during drag
    private Vector3 fingerWorldPos;

    private void Awake()
    {
        Instance = this;

        shaft = CreateLine("TempShaft", Color.cyan, 0.01f);
        arrowLeft = CreateLine("TempArrowLeft", Color.cyan, 0.01f);
        arrowRight = CreateLine("TempArrowRight", Color.cyan, 0.01f);

        dragShaft = CreateLine("DragShaft", Color.cyan, 0.006f);
        dragArrowLeft = CreateLine("DragArrowLeft", Color.cyan, 0.006f);
        dragArrowRight = CreateLine("DragArrowRight", Color.cyan, 0.006f);

        SetArrowsEnabled(false);
        SetDragArrowEnabled(false);
    }

    private LineRenderer CreateLine(string objName, Color color, float width)
    {
        GameObject obj = new GameObject(objName);
        obj.transform.SetParent(transform);
        LineRenderer lr = obj.AddComponent<LineRenderer>();

        Shader shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");

        lr.material = new Material(shader);
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.enabled = false;
        return lr;
    }

    private void SetArrowsEnabled(bool on)
    {
        if (shaft != null) shaft.enabled = on;
        if (arrowLeft != null) arrowLeft.enabled = on;
        if (arrowRight != null) arrowRight.enabled = on;
    }

    private void SetDragArrowEnabled(bool on)
    {
        if (dragShaft != null) dragShaft.enabled = on;
        if (dragArrowLeft != null) dragArrowLeft.enabled = on;
        if (dragArrowRight != null) dragArrowRight.enabled = on;
    }

    public void PlaceAtStart(Vector3 locomotivePos)
    {
        // Use the parent's right axis if available so it
        // aligns with the train regardless of orientation
        Vector3 rightAxis = transform.parent != null
            ? transform.parent.right
            : Vector3.right;

        targetPosition = locomotivePos +
            Vector3.up * floatHeight +
            rightAxis * 0.1f;

        transform.position = targetPosition;
        isPlaced = true;
        UpdateLabel();
        SetArrowsEnabled(false);
    }

    // Called from PointerDrag while finger is moving
    public void UpdateDragArrow(Vector3 worldFingerPos)
    {
        fingerWorldPos = worldFingerPos;
        SetDragArrowEnabled(true);
        SetArrowsEnabled(false); // hide normal arrow while dragging
        DrawArrow(dragShaft, dragArrowLeft, dragArrowRight,
            transform.position, fingerWorldPos);
    }

    // Called from PointerDrag when finger is released
    public void EndDrag()
    {
        isDragging = false;
        SetDragArrowEnabled(false);

        // Restore normal arrow if pointing at something
        if (pointingAt != null)
        {
            SetArrowsEnabled(true);
            UpdateArrow();
        }
    }

    private void Update()
    {
        if (!isPlaced) return;
        if (isDragging) return; // don't move while dragging

        // Always face camera
        Camera cam = Camera.main;
        if (cam != null)
        {
            transform.LookAt(
                transform.position + cam.transform.rotation * Vector3.forward,
                cam.transform.rotation * Vector3.up);
        }

        if (pointingAt != null)
            UpdateArrow();
    }

    public void PointAt(Node node)
    {
        
        pointingAt = node;

        if (node == null)
        {
            SetArrowsEnabled(false);
            UpdateLabel();
            return;
        }

        // Don't move TempPointer — just update the arrow to point at node in place
        SetArrowsEnabled(true);
        UpdateLabel();
        UpdateArrow();

        if (LinkedListStatisticsTracker.Instance != null)
            LinkedListStatisticsTracker.Instance.RecordTempPointerUse();

        LinkedListGameManager gm = LinkedListGameManager.Instance;
        if (gm != null)
        {
            gm.SetTempPriority(true);
            gm.UpdateReachability();
        }
    }

    private void UpdateArrow()
    {
        if (pointingAt == null) return;
        Vector3 to = pointingAt.transform.position + Vector3.up * 0.05f;
        DrawArrow(shaft, arrowLeft, arrowRight, transform.position, to);
    }

    private void DrawArrow(LineRenderer s, LineRenderer al, LineRenderer ar,
        Vector3 from, Vector3 to)
    {
        s.SetPosition(0, from);
        s.SetPosition(1, to);

        Vector3 dir = (to - from).normalized;
        if (dir == Vector3.zero) return;

        float headLength = 0.04f;

        Vector3 right = Quaternion.Euler(0, 25f, 0) * (-dir) * headLength;
        Vector3 left = Quaternion.Euler(0, -25f, 0) * (-dir) * headLength;

        ar.SetPosition(0, to);
        ar.SetPosition(1, to + right);
        al.SetPosition(0, to);
        al.SetPosition(1, to + left);
    }

    private void UpdateLabel()
    {
        if (label == null) return;
        label.text = pointingAt != null
            ? "temp\n= " + pointingAt.GetColorName()
            : "temp\n= null";
    }
}