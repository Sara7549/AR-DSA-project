using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class DragHandler : MonoBehaviour
{
    [Header("References")]
    public QueueController queueController;
    public Camera arCamera;

    [Header("Settings")]
    public float dragThreshold = 0.02f;
    public float liftHeight = 0.05f;

    private VehicleVisual draggedVehicle = null;
    private Vector2 touchStartPos;
    private Vector3 originalWorldPos;
    private bool isDragging = false;
    private bool isActive = false;

    private LaneVisual[] laneVisuals;
    private HoldingAreaVisual holdingVisual;
    private ExitZoneVisual exitVisual;

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

    public void SetActive(bool active)
    {
        isActive = active;
        if (active)
        {
            laneVisuals = FindObjectsOfType<LaneVisual>();
            holdingVisual = FindObjectOfType<HoldingAreaVisual>();
            exitVisual = FindObjectOfType<ExitZoneVisual>();
        }
    }

    private Vehicle liftedVehicle = null; // add this field at the top

    private void OnFingerDown(Finger finger)
    {
        if (!isActive) return;
        if (finger.index != 0) return;

        Vector2 screenPos = finger.screenPosition;
        if (arCamera == null) arCamera = Camera.main;
        if (arCamera == null) return;

        Ray ray = arCamera.ScreenPointToRay(screenPos);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
        System.Array.Sort(hits, (a, b) =>
            a.distance.CompareTo(b.distance));

        VehicleVisual found = null;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.isTrigger) continue;
            VehicleVisual vv =
                hit.collider.GetComponentInParent<VehicleVisual>();
            if (vv != null) { found = vv; break; }
        }

        if (found != null)
        {
            // Only lift from lane (not holding area — laneIndex -1)
            if (found.laneIndex >= 0)
            {
                bool lifted =
                    queueController.TryLiftFromLane(found.laneIndex);
                if (!lifted) return; // lane was empty, abort
                liftedVehicle = found.vehicle;
            }
            else
            {
                liftedVehicle = found.vehicle;
            }

            draggedVehicle = found;
            touchStartPos = screenPos;
            originalWorldPos = found.transform.position;

            found.transform.position =
                originalWorldPos + Vector3.up * liftHeight;
            found.StartDrag();
            isDragging = false;

            // Slide remaining vehicles forward
            if (found.laneIndex >= 0)
            {
                LaneVisual lv =
                    queueController.GetLaneVisual(found.laneIndex);
                if (lv != null)
                    lv.SlideRemainingForward(
                        found.slotIndex, found.vehicle.SlotSize);
            }
        }
    }

    private void OnFingerUp(Finger finger)
    {
        if (!isActive) return;
        if (finger.index != 0) return;
        if (draggedVehicle == null) return;

        int sourceLane = draggedVehicle.laneIndex;
        bool moveSucceeded = false;

        if (isDragging)
        {
            string destination =
                GetDropDestination(finger.screenPosition);

            if (destination.StartsWith("Lane"))
            {
                int toLane = int.Parse(
                    destination.Replace("Lane", ""));

                if (sourceLane == -1)
                {
                    // From holding — game state still has it there
                    moveSucceeded = queueController.TryMoveFromHolding(
                        liftedVehicle, toLane);
                }
                else
                {
                    // Already lifted from source lane in OnFingerDown,
                    // so just enqueue into destination
                    moveSucceeded = queueController.TryEnqueueToLane(
                        liftedVehicle, toLane);
                }
            }
            else if (destination == "Holding")
            {
                if (sourceLane >= 0)
                {
                    // Already dequeued, just add to holding
                    moveSucceeded = queueController.TryAddToHolding(
                        liftedVehicle);
                }
            }
            else if (destination == "Exit")
            {
                if (sourceLane >= 0 && liftedVehicle.isTarget)
                {
                    moveSucceeded = queueController.TryExitLifted(
                        liftedVehicle);
                }
            }
        }

        if (!moveSucceeded)
        {
            // Return vehicle to its original lane in game state
            if (sourceLane >= 0)
            {
                queueController.ReturnVehicleToLane(
                    sourceLane, liftedVehicle);

                LaneVisual lv =
                    queueController.GetLaneVisual(sourceLane);
                if (lv != null)
                    lv.RestorePositions();
            }
            draggedVehicle.ReturnToOriginal();
        }
        else
        {
            // Move succeeded — rebuild visuals cleanly
            queueController.RenderAll();
            queueController.UpdateMoveCount();
            queueController.CheckWinPublic();
        }

        draggedVehicle.EndDrag();
        draggedVehicle = null;
        liftedVehicle = null;
        isDragging = false;
    }

    private void OnFingerMove(Finger finger)
    {
        if (!isActive) return;
        if (finger.index != 0) return;
        if (draggedVehicle == null) return;

        Vector2 screenPos = finger.screenPosition;
        float effectiveDpi = Mathf.Max(Screen.dpi, 160f);
        float distance = Vector2.Distance(screenPos, touchStartPos);

        if (distance > dragThreshold * effectiveDpi)
            isDragging = true;

        if (isDragging)
        {
            Ray ray = arCamera.ScreenPointToRay(screenPos);
            Plane plane = new Plane(Vector3.up,
                originalWorldPos + Vector3.up * liftHeight);

            float enter;
            if (plane.Raycast(ray, out enter))
                draggedVehicle.transform.position = ray.GetPoint(enter);
        }
    }

    

    private string GetDropDestination(Vector2 screenPos)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPos);

        // RaycastAll so the lifted vehicle doesn't block zone detection,
        // and QueryTriggerInteraction.Collide so trigger zones are included
        RaycastHit[] hits = Physics.RaycastAll(
            ray, 100f, ~0, QueryTriggerInteraction.Collide);

        // Sort by distance so we check closest first
        System.Array.Sort(hits, (a, b) =>
            a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            // Skip the vehicle being dragged
            if (draggedVehicle != null &&
                hit.collider.transform.IsChildOf(
                    draggedVehicle.transform))
                continue;

            LaneVisual lane =
                hit.collider.GetComponentInParent<LaneVisual>();
            if (lane != null)
                return "Lane" + lane.laneIndex;

            HoldingAreaVisual holding =
                hit.collider.GetComponentInParent<HoldingAreaVisual>();
            if (holding != null)
                return "Holding";

            ExitZoneVisual exit =
                hit.collider.GetComponentInParent<ExitZoneVisual>();
            if (exit != null)
                return "Exit";
        }

        return "None";
    }
}