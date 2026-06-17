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

    private Vehicle liftedVehicle = null;
    private int sourceLaneIndex = -1; // stored separately — not from VehicleVisual

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

        // Find the closest VehicleVisual to the tap (screen-space distance)
        VehicleVisual found = null;
        float closestScreenDist = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.isTrigger) continue;

            VehicleVisual vv =
                hit.collider.GetComponentInParent<VehicleVisual>();
            if (vv == null) continue;

            Vector3 screenPoint = arCamera.WorldToScreenPoint(
                vv.transform.position);
            float screenDist = Vector2.Distance(
                screenPos,
                new Vector2(screenPoint.x, screenPoint.y));

            if (screenDist < closestScreenDist)
            {
                closestScreenDist = screenDist;
                found = vv;
            }
        }

        if (found == null) return;


        // Only allow lifting the FRONT vehicle (slotIndex == 0).
        // Tapping a middle or back vehicle is invalid in a queue.
        // Replace the slotIndex check in OnFingerDown with this:
        if (found.laneIndex >= 0 && found.slotIndex != 0)
        {
            // Double-check against actual lane data —
            // collider overlap can cause wrong vehicle to be hit
            QueueGameManager gm = QueueGameManager.Instance;
            Vehicle actualFront = gm.lanes[found.laneIndex].Front;

            if (actualFront != found.vehicle)
            {
                if (QueueStatisticsTracker.Instance != null)
                    QueueStatisticsTracker.Instance.RecordInvalidMove();
                queueController.ShowFeedback(
                    "Can only dequeue from the front of the queue!");
                return;
            }
            // If vehicle matches front despite slotIndex != 0,
            // it's a multi-slot vehicle at the front — allow it
        }


        // Store the source lane BEFORE we modify the visual
        sourceLaneIndex = found.laneIndex;

        if (found.laneIndex >= 0)
        {
            bool lifted =
                queueController.TryLiftFromLane(found.laneIndex);
            if (!lifted) return;

            liftedVehicle = found.vehicle;

            
            // Clear the laneIndex on the visual immediately after lifting
            // so RenderAll() won't treat this object as still-in-lane.
            found.laneIndex = -2; // sentinel: "lifted, not in holding either"
           
        }
        else
        {
            // Vehicle is from holding area (laneIndex == -1)
            liftedVehicle = found.vehicle;
        }

        draggedVehicle = found;
        touchStartPos = screenPos;
        originalWorldPos = found.transform.position;

        found.transform.position =
            originalWorldPos + Vector3.up * liftHeight;
        found.StartDrag();
        isDragging = false;

        if (sourceLaneIndex >= 0)
        {
            LaneVisual lv =
                queueController.GetLaneVisual(sourceLaneIndex);
            if (lv != null)
                lv.SlideRemainingForward(
                    found.slotIndex, found.vehicle.SlotSize);
        }
    }

    private void OnFingerUp(Finger finger)
    {
        if (!isActive) return;
        if (finger.index != 0) return;
        if (draggedVehicle == null) return;

        bool moveSucceeded = false;

        if (isDragging)
        {
            string destination = GetDropDestination(finger.screenPosition);

            if (destination == "LaneBack0" ||
                destination == "LaneBack1" ||
                destination == "LaneBack2")
            {
                int toLane = int.Parse(destination.Replace("LaneBack", ""));

                if (sourceLaneIndex == -1)
                    moveSucceeded = queueController.TryMoveFromHolding(liftedVehicle, toLane);
                else
                    moveSucceeded = queueController.TryEnqueueToLane(liftedVehicle, toLane);
                // RecordMove() / RecordInvalidMove() handled inside those methods
            }
            else if (destination == "LaneMiddle0" ||
                     destination == "LaneMiddle1" ||
                     destination == "LaneMiddle2")
            {
                // Tried to insert in the middle — invalid queue operation
                // RecordInvalidMove() is called below in the !moveSucceeded block
                queueController.ShowFeedback("Queues only accept vehicles at the back!");
                moveSucceeded = false;
            }
            else if (destination == "Holding")
            {
                if (sourceLaneIndex >= 0)
                    moveSucceeded = queueController.TryAddToHolding(liftedVehicle);
                // TryAddToHolding calls RecordHoldingViolation() on failure internally
                // Don't record a move here — holding is not a queue operation
            }
            else if (destination == "Exit")
            {
                if (liftedVehicle != null && liftedVehicle.isTarget)
                {
                    moveSucceeded = queueController.TryExitLifted(liftedVehicle);
                    if (moveSucceeded)
                    {
                        if (QueueStatisticsTracker.Instance != null)
                            QueueStatisticsTracker.Instance.RecordMove();
                    }
                    // failure handled below — RecordFrontAccessViolation via !moveSucceeded
                    // is too generic here, so record it specifically:
                    else
                    {
                        if (QueueStatisticsTracker.Instance != null)
                            QueueStatisticsTracker.Instance.RecordFrontAccessViolation();
                    }
                }
                else
                {
                    queueController.ShowFeedback("Only the target car can exit!");
                    moveSucceeded = false;
                    // falls through to RecordInvalidMove() below
                }
            }
            // destination == "None" falls through with moveSucceeded = false
            // and gets recorded below

            // ── Record any failed drag as an invalid move ──────────────────────
            // This catches: None, LaneMiddle, bad Exit, failed Holding — everything
            // that didn't succeed. Holding violations are already tracked separately
            // via RecordHoldingViolation(), so we skip that case.
            if (!moveSucceeded && destination != "Holding")
            {
                if (QueueStatisticsTracker.Instance != null)
                    QueueStatisticsTracker.Instance.RecordInvalidMove();
            }
        }

        if (!moveSucceeded)
        {
            if (sourceLaneIndex >= 0)
            {
                queueController.ReturnVehicleToLane(sourceLaneIndex, liftedVehicle);
                LaneVisual lv = queueController.GetLaneVisual(sourceLaneIndex);
                if (lv != null)
                    lv.RestorePositions();
            }

            if (draggedVehicle != null && sourceLaneIndex >= 0)
                draggedVehicle.laneIndex = sourceLaneIndex;

            draggedVehicle.ReturnToOriginal();
        }
        else
        {
            draggedVehicle.EndDrag();
            draggedVehicle = null;
            liftedVehicle = null;

            queueController.RenderAll();
            queueController.UpdateMoveCount();
            queueController.CheckWinPublic();
            queueController.CheckExitPrompt();
            return;
        }

        draggedVehicle.EndDrag();
        draggedVehicle = null;
        liftedVehicle = null;
        sourceLaneIndex = -1;
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

    // 
    // GetDropDestination now returns:
    //   "LaneBack{i}"   — drop is at or behind the last occupied slot (valid)
    //   "LaneMiddle{i}" — drop is in front of the last occupied slot (invalid)
    //   "Holding"
    //   "Exit"
    //   "None"
    private string GetDropDestination(Vector2 screenPos)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPos);

        RaycastHit[] hits = Physics.RaycastAll(
            ray, 100f, ~0, QueryTriggerInteraction.Collide);

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
            {
                return ClassifyLaneDrop(lane, screenPos);
            }

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

    /// <summary>
    /// Returns "LaneBack{i}" if the drop point is at or behind the last
    /// occupied slot (the correct enqueue position), or "LaneMiddle{i}"
    /// if the drop is anywhere in front of that (invalid insertion).
    /// </summary>
    private string ClassifyLaneDrop(LaneVisual lane, Vector2 screenPos)
    {
        int idx = lane.laneIndex;

        // Get the world-space position of the drop point on the lane plane
        Ray ray = arCamera.ScreenPointToRay(screenPos);
        Plane lanePlane = new Plane(Vector3.up, lane.transform.position);
        float enter;
        Vector3 dropWorldPos = Vector3.zero;
        if (lanePlane.Raycast(ray, out enter))
            dropWorldPos = ray.GetPoint(enter);

        // Find which slot the drop is closest to
        Transform[] slots = lane.slots;
        if (slots == null || slots.Length == 0)
            return "LaneBack" + idx; // can't tell, assume back

        // The "back" of the queue is the LAST slot (highest index).
        // In a queue, vehicles enter at the back.
        // We compare the drop's position along the lane's forward axis
        // against the last occupied slot.

        // Get the lane's local forward (from slot[0] toward slot[last])
        Vector3 laneForward = Vector3.zero;
        if (slots.Length > 1 && slots[0] != null && slots[slots.Length - 1] != null)
            laneForward = (slots[slots.Length - 1].position
                           - slots[0].position).normalized;
        else
            return "LaneBack" + idx; // single slot, always back

        // Project each slot and the drop onto the lane axis
        float dropProj = Vector3.Dot(dropWorldPos, laneForward);

        // Find the last slot that is actually occupied
        // (use lane's game data via QueueGameManager)
        QueueGameManager gm = QueueGameManager.Instance;
        if (gm == null || idx < 0 || idx >= gm.lanes.Length)
            return "LaneBack" + idx;

        QueueLane queueLane = gm.lanes[idx];
        int occupiedSlots = 0;
        foreach (Vehicle v in queueLane.vehicles)
            occupiedSlots += v.SlotSize;

        // Back slot = slot index (occupiedSlots), clamped to last slot
        int backSlotIndex = Mathf.Min(occupiedSlots, slots.Length - 1);
        Transform backSlotTransform = slots[backSlotIndex];
        if (backSlotTransform == null)
            return "LaneBack" + idx;

        float backSlotProj =
            Vector3.Dot(backSlotTransform.position, laneForward);

        // If drop is on the back-side of the last occupied slot ? valid back
        // Allow a half-slot tolerance so the user doesn't have to be pixel-perfect
        float slotTolerance = lane.slotSize * 0.5f;
        if (dropProj >= backSlotProj - slotTolerance)
            return "LaneBack" + idx;
        else
            return "LaneMiddle" + idx;
    }
    
}