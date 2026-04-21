using UnityEngine;

public class Node : MonoBehaviour
{
    public int value;
    public Node next;
    public bool isReachable;

    // We create these in Awake — no need to assign in Inspector
    private LineRenderer shaft;
    private LineRenderer arrowLeft;
    private LineRenderer arrowRight;

    private void Awake()
    {
        shaft = CreateLine("Shaft", Color.yellow, 0.008f);
        arrowLeft = CreateLine("ArrowLeft", Color.yellow, 0.008f);
        arrowRight = CreateLine("ArrowRight", Color.yellow, 0.008f);

        SetArrowsEnabled(false);
    }

    private LineRenderer CreateLine(string objName,
     Color color, float width)
    {
        GameObject obj = new GameObject(objName);
        obj.transform.SetParent(transform);
        LineRenderer lr = obj.AddComponent<LineRenderer>();

        // Use the built-in default line material
        lr.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));

        // Fallback if that also fails
        if (lr.material == null || lr.material.shader == null)
            lr.material = new Material(Shader.Find("Hidden/InternalErrorShader"));

        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.enabled = false;
        return lr;
    }
    private void SetArrowsEnabled(bool enabled)
    {
        if (shaft != null) shaft.enabled = enabled;
        if (arrowLeft != null) arrowLeft.enabled = enabled;
        if (arrowRight != null) arrowRight.enabled = enabled;
    }

    public void SetNext(Node newNext)
    {
        next = newNext;
        UpdatePointerVisual();
    }

    public void UpdatePointerVisual()
    {
        if (next == null)
        {
            SetArrowsEnabled(false);
            return;
        }

        SetArrowsEnabled(true);

        // Positions slightly above models so line is visible
        Vector3 from = transform.position +
            Vector3.up * 0.12f;
        Vector3 to = next.transform.position +
            Vector3.up * 0.12f;

        // Draw shaft
        shaft.SetPosition(0, from);
        shaft.SetPosition(1, to );

        // Draw arrowhead — two lines forming a V at the tip
        Vector3 dir = (to - from).normalized;
        float headLength = 0.04f;
        float headAngle = 25f;

        // Rotate direction for left and right head lines
        Vector3 right = Quaternion.Euler(0, headAngle, 0)
            * (-dir) * headLength;
        Vector3 left = Quaternion.Euler(0, -headAngle, 0)
            * (-dir) * headLength;

        arrowRight.SetPosition(0, to);
        arrowRight.SetPosition(1, to + right);

        arrowLeft.SetPosition(0, to);
        arrowLeft.SetPosition(1, to + left);
    }

    public void SetReachable(bool reachable)
    {
        isReachable = reachable;

        // Change arrow color based on reachability
        Color arrowColor = reachable ? Color.yellow : Color.red;
        if (shaft != null)
        {
            shaft.startColor = arrowColor;
            shaft.endColor = arrowColor;
        }
        if (arrowLeft != null)
        {
            arrowLeft.startColor = arrowColor;
            arrowLeft.endColor = arrowColor;
        }
        if (arrowRight != null)
        {
            arrowRight.startColor = arrowColor;
            arrowRight.endColor = arrowColor;
        }
    }
    // Add these fields to Node.cs
    private Vector3 targetPosition;
    private bool isSliding = false;
    private float slideSpeed = 5f;

    private string slideDebug = "";
    private int updateCount = 0;

    private string slideLog = "";

    private void OnGUI()
    {
        if (slideLog == "") return;

        float yOffset = value * 120f;
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 24;
        style.normal.textColor = Color.cyan;

        GUI.Label(new Rect(10, yOffset, 600, 110), slideLog, style);
    }

    public void SlideToPosition(Vector3 target, float speed)
    {
        targetPosition = target;
        slideSpeed = speed;
        isSliding = true;
    }

    private void Update()
    {
        if (!isSliding) return;

        // Convert world target to local space of parent
        Vector3 localTarget = transform.parent != null
            ? transform.parent.InverseTransformPoint(targetPosition)
            : targetPosition;

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            localTarget,
            Time.deltaTime * slideSpeed);

        UpdatePointerVisual();

        if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
        {
            transform.localPosition = localTarget;
            isSliding = false;
        }
    }
    private void FixedUpdate()
    {
        if (!isSliding) return;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * slideSpeed);

        UpdatePointerVisual();

        if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
        {
            transform.position = targetPosition;
            isSliding = false;
        }
    }
}