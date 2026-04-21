using UnityEngine;
using TMPro;

public class TempPointer : MonoBehaviour
{
    public static TempPointer Instance;

    [Header("References")]
    public TextMeshProUGUI label;  // drag the TMP text here

    [Header("Settings")]
    public float floatHeight = 0.5f;
    public float moveSpeed = 8f;

    public Node pointingAt { get; private set; }

    private LineRenderer shaft;
    private LineRenderer arrowLeft;
    private LineRenderer arrowRight;

    private Vector3 targetPosition;
    private bool isPlaced = false;

    private void Awake()
    {
        Instance = this;

        // Create arrow lines on this GameObject directly
        shaft = CreateLine("TempShaft", Color.cyan, 0.01f);   // thicker
        arrowLeft = CreateLine("TempArrowLeft", Color.cyan, 0.01f);
        arrowRight = CreateLine("TempArrowRight", Color.cyan, 0.01f);

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

    private void SetArrowsEnabled(bool on)
    {
        if (shaft != null) shaft.enabled = on;
        if (arrowLeft != null) arrowLeft.enabled = on;
        if (arrowRight != null) arrowRight.enabled = on;
    }

    public void PlaceAtStart(Vector3 locomotivePos)
    {
        // Float above and slightly to the side of locomotive
        targetPosition = locomotivePos +
            Vector3.up * floatHeight +
            Vector3.right * 0.1f;
        transform.position = targetPosition;
        isPlaced = true;

        UpdateLabel();
        SetArrowsEnabled(false);
    }

    private void Update()
    {
        if (!isPlaced) return;

        // Smooth move to target
        transform.position = Vector3.Lerp(
            transform.position, targetPosition,
            Time.deltaTime * moveSpeed);

        // Always face camera
        Camera cam = Camera.main;
        if (cam != null)
        {
            transform.LookAt(
                transform.position +
                cam.transform.rotation * Vector3.forward,
                cam.transform.rotation * Vector3.up);
        }

        // Update arrow every frame
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

        targetPosition = node.transform.position +
            Vector3.up * floatHeight;

        SetArrowsEnabled(true);
        UpdateLabel();
        UpdateArrow();
    }

    private void UpdateArrow()
    {
        if (pointingAt == null) return;

        Vector3 from = transform.position;
        Vector3 to = pointingAt.transform.position +
            Vector3.up * 0.05f;

        shaft.SetPosition(0, from);
        shaft.SetPosition(1, to);

        Vector3 dir = (to - from).normalized;
        float headLength = 0.04f;
        float headAngle = 25f;

        Vector3 right = Quaternion.Euler(0, headAngle, 0)
            * (-dir) * headLength;
        Vector3 left = Quaternion.Euler(0, -headAngle, 0)
            * (-dir) * headLength;

        arrowRight.SetPosition(0, to);
        arrowRight.SetPosition(1, to + right);
        arrowLeft.SetPosition(0, to);
        arrowLeft.SetPosition(1, to + left);
    }

    private void UpdateLabel()
    {
        if (label == null) return;

        if (pointingAt != null)
            label.text = "temp\n= C_" + pointingAt.value;
        else
            label.text = "temp\n= null";
    }
}