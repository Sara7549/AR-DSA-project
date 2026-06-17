using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class StackMarkerSpawner : MonoBehaviour
{
    public ARTrackedImageManager trackedImageManager;
    public ARPlacement placementScript;
    public GameObject reticle;

    private bool markerDetected = false;

    private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        if (markerDetected) return;

        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            markerDetected = true;

            if (placementScript != null)
                placementScript.enabled = false;

            if (reticle != null)
                reticle.SetActive(false);

            placementScript.PlaceFromMarker(trackedImage.transform);
        }
    }
}