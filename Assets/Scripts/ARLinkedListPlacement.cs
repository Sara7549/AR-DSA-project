using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class ARLinkedListPlacement : MonoBehaviour
{
    [Header("AR")]
    public LinkedListGameManager gameManager; // drag scene object here, remove linkedListPrefab
    public ARRaycastManager raycastManager;
    [Header("Reticle")]
    public GameObject reticlePrefab;
    [Header("UI")]
    public LinkedListUIManager uiManager;

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
        if (hasPlaced) return;

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
                uiManager.SetStatePlace();
        }
        else
        {
            if (reticleInstance != null)
                reticleInstance.SetActive(false);

            if (uiManager != null)
                uiManager.SetStateScan();
        }
    }

    // This replaces your old PlaceObject entirely
    void PlaceObject(Vector2 touchPosition)
    {
        if (hasPlaced) return;
        if (raycastManager.Raycast(touchPosition, hits, PlaneTypes))
        {
            Pose hitPose = hits[0].pose;

            // Always align train sideways relative to camera
            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0;
            cameraForward.Normalize();

            // Train runs along camera's right axis so it appears sideways
            Quaternion trainRotation = Quaternion.LookRotation(cameraForward);

            gameManager.transform.position = hitPose.position;
            gameManager.transform.rotation = trainRotation;

            hasPlaced = true;

            if (reticleInstance != null)
                reticleInstance.SetActive(false);

            gameManager.SetupInitialList();

            PointerDrag pd = gameManager.GetComponent<PointerDrag>();
            if (pd != null)
            {
                pd.gameManager = gameManager;
                pd.arCamera = Camera.main;
            }

            if (uiManager != null)
                uiManager.OnListPlaced(gameManager.gameObject);
        }
    }
}