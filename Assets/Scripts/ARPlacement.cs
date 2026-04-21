using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class ARPlacement : MonoBehaviour
{
    [Header("AR")]
    public GameObject stackGroupPrefab;
    public ARRaycastManager raycastManager;

    [Header("Reticle")]
    public GameObject reticlePrefab;

    [Header("UI")]
    public UIManager uiManager;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject currentObject;
    private GameObject reticleInstance;
    private bool hasPlaced = false;

    private Vector3 placedPosition;
    private Quaternion placedRotation;

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
            TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            if (reticleInstance != null)
            {
                reticleInstance.SetActive(true);
                if (uiManager != null)
                    uiManager.SetStatePlace();
                reticleInstance.transform.position = new Vector3(
                    hitPose.position.x,
                    hitPose.position.y + 0.01f,
                    hitPose.position.z);
                reticleInstance.transform.rotation = hitPose.rotation;
            }
        }
        else
        {
            if (reticleInstance != null)
                reticleInstance.SetActive(false);
            if (uiManager != null)
                uiManager.SetStateScan();
        }
    }

    void PlaceObject(Vector2 touchPosition)
    {
        if (hasPlaced) return;

        if (raycastManager.Raycast(touchPosition, hits,
            TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            placedPosition = hitPose.position;
            placedRotation = hitPose.rotation;

            // Instantiate prefab
            currentObject = Instantiate(stackGroupPrefab,
                placedPosition, placedRotation);

            currentObject.transform.SetParent(null);
            hasPlaced = true;

            // Hide reticle
            if (reticleInstance != null)
                reticleInstance.SetActive(false);

            //  Get GameController FIRST
            GameController gameController =
                FindObjectOfType<GameController>();

            if (gameController != null)
            {
                //  Get visuals ONLY ONCE
                StackVisual[] visuals =
                    currentObject.GetComponentsInChildren<StackVisual>();

                // CENTER STACKS
                if (visuals.Length == 3)
                {
                    Vector3 centerOffset =
                        visuals[1].transform.localPosition;

                    foreach (StackVisual v in visuals)
                    {
                        v.transform.localPosition -= centerOffset;
                    }
                }

                // Assign + initialize
                gameController.stackVisuals = visuals;
                gameController.InitializeGame();

                // Activate tap detectors
                StackTapDetector[] detectors =
                    currentObject.GetComponentsInChildren<StackTapDetector>();

                foreach (StackTapDetector detector in detectors)
                    detector.SetActive(true);
            }

            // UI update
            if (uiManager != null)
                uiManager.OnStackPlaced(currentObject);
        }
    }
}