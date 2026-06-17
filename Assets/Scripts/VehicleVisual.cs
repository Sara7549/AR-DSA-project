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

    public bool useLocalSpace = false;   // set true in marker mode

    private void Update()
    {
        if (!isDragging)
        {
            if (useLocalSpace)
                transform.localPosition = Vector3.Lerp(
                    transform.localPosition, targetPosition,
                    Time.deltaTime * slideSpeed);
            else
                transform.position = Vector3.Lerp(
                    transform.position, targetPosition,
                    Time.deltaTime * slideSpeed);
        }
    }

    public void SetOriginalPosition(Vector3 pos)
    {
        originalPosition = pos;
        targetPosition = pos;
        if (useLocalSpace)
            transform.localPosition = pos;
        else
            transform.position = pos;
    }

    public void SetTargetPosition(Vector3 pos)
    {
        targetPosition = pos;
        originalPosition = pos;
    }

    public void ReturnToOriginal()
    {
        targetPosition = originalPosition;
        isDragging = false;
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

    public bool IsDragging() => isDragging;
}