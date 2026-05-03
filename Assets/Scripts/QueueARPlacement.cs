using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class QueueARPlacement : MonoBehaviour
{
    [Header("AR")]
    public GameObject queueGroupPrefab;
    public ARRaycastManager raycastManager;

    [Header("Reticle")]
    public GameObject reticlePrefab;

    [Header("UI")]
    public QueueUIManager uiManager;

    private static readonly TrackableType PlaneTypes =
    TrackableType.PlaneWithinPolygon |
    TrackableType.PlaneWithinBounds |
    TrackableType.PlaneWithinInfinity;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject currentObject;
    private GameObject reticleInstance;
    private bool hasPlaced = false;

    private Vector3 placedPosition;
    private Quaternion placedRotation;
    private Vector3 originalScale;
    private Vector3 originalPosition;

    private void Start()
    {
        if (reticlePrefab != null)
        {
            reticleInstance = Instantiate(reticlePrefab);
            reticleInstance.SetActive(false);
        }
    }

    private void OnEnable()
    {
        InputHandler.OnTap += PlaceObject;
    }

    private void OnDisable()
    {
        InputHandler.OnTap -= PlaceObject;
    }

    private void Update()
    {
        if (hasPlaced)
        {
            if (currentObject != null)
            {
                currentObject.transform.position = placedPosition;
                currentObject.transform.rotation = placedRotation;
            }
            return;
        }

        Vector2 screenCenter = new Vector2(
            Screen.width / 2f, Screen.height / 2f);

        if (raycastManager.Raycast(screenCenter, hits,
            PlaneTypes))
        {
            Pose hitPose = hits[0].pose;

            if (reticleInstance != null)
            {
                reticleInstance.SetActive(true);
                reticleInstance.transform.position = new Vector3(
                    hitPose.position.x,
                    hitPose.position.y + 0.01f,
                    hitPose.position.z);
                reticleInstance.transform.rotation = hitPose.rotation;
            }

            if (uiManager != null)
                uiManager.SetStateScan();
        }
        else
        {
            if (reticleInstance != null)
                reticleInstance.SetActive(false);
        }
    }

    void PlaceObject(Vector2 touchPosition)
    {
        if (hasPlaced) return;

        if (raycastManager.Raycast(touchPosition, hits,
            PlaneTypes))
        {
            Pose hitPose = hits[0].pose;
            placedPosition = hitPose.position;
            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0f; // keep it flat
            cameraForward.Normalize();

            placedRotation = Quaternion.LookRotation(cameraForward); 

            currentObject = Instantiate(queueGroupPrefab,
                placedPosition, placedRotation);

            currentObject.transform.SetParent(null);
            hasPlaced = true;

            if (reticleInstance != null)
                reticleInstance.SetActive(false);

            // Find and initialize QueueController
            QueueController queueController =
                FindObjectOfType<QueueController>();

            if (queueController != null)
            {
                LaneVisual[] laneVisuals =
                    currentObject.GetComponentsInChildren<LaneVisual>();

                HoldingAreaVisual holdingVisual =
                    currentObject.GetComponentInChildren<HoldingAreaVisual>();

                ExitZoneVisual exitVisual =
                    currentObject.GetComponentInChildren<ExitZoneVisual>();

                queueController.laneVisuals = laneVisuals;
                queueController.holdingVisual = holdingVisual;
                queueController.exitVisual = exitVisual;
                queueController.InitializeGame();
            }

            if (uiManager != null)
                uiManager.OnGamePlaced(currentObject);

            DragHandler dragHandler = FindObjectOfType<DragHandler>();
            if (dragHandler != null)
            {
                dragHandler.arCamera = Camera.main;
                dragHandler.SetActive(true);
            }
        }
    }
}