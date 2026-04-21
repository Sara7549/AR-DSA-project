using UnityEngine;

public class VehicleVisual : MonoBehaviour
{
    public Vehicle vehicle;
    public int laneIndex;
    public int slotIndex;

    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private bool isDragging = false;

    // Smoothing speed — increase for snappier, decrease for floatier
    public float slideSpeed = 4f;

    private void Update()
    {
        // Smoothly move toward target when not dragging
        if (!isDragging)
        {
            transform.position = Vector3.Lerp(
                transform.position, targetPosition,
                Time.deltaTime * slideSpeed);
        }
    }

    public void SetOriginalPosition(Vector3 pos)
    {
        originalPosition = pos;
        targetPosition = pos;
        // Snap immediately on first placement
        transform.position = pos;
    }

    public void SetTargetPosition(Vector3 pos)
    {
        targetPosition = pos;
        // Also update original so ReturnToOriginal
        // goes back to the right slot
        originalPosition = pos;
    }

    public Vector3 GetOriginalPosition() => originalPosition;

    public void StartDrag()
    {
        isDragging = true;
    }

    public void EndDrag()
    {
        isDragging = false;
    }

    public void ReturnToOriginal()
    {
        targetPosition = originalPosition;
        isDragging = false;
    }

    public bool IsDragging() => isDragging;
}