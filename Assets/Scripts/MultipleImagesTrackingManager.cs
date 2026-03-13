using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MultipleImagesTrackingManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> prefabsToSpawn = new List<GameObject>();

    private ARTrackedImageManager _trackedImageManager;
    private Dictionary<string, GameObject> _arObjects;

    private void Start()
    {
        _trackedImageManager = GetComponent<ARTrackedImageManager>();

        if (_trackedImageManager == null) return;

        _trackedImageManager.trackedImagesChanged += OnImagesTrackedChanged;

        _arObjects = new Dictionary<string, GameObject>();
        SetupSceneElements();
    }

    private void OnDestroy()
    {
        if (_trackedImageManager != null)
            _trackedImageManager.trackedImagesChanged -= OnImagesTrackedChanged;
    }

    private void SetupSceneElements()
    {
        foreach (GameObject prefab in prefabsToSpawn)
        {
            var arObject = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            arObject.name = prefab.name;
            arObject.SetActive(false);
            _arObjects.Add(arObject.name, arObject);
        }
    }

    private void OnImagesTrackedChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
            UpdateTrackedImages(trackedImage);

        foreach (var trackedImage in eventArgs.updated)
            UpdateTrackedImages(trackedImage);

        foreach (var trackedImage in eventArgs.removed)
            _arObjects[trackedImage.referenceImage.name].SetActive(false);
    }

    private void UpdateTrackedImages(ARTrackedImage trackedImage)
    {
        if (trackedImage == null) return;

        if (trackedImage.trackingState == TrackingState.Limited ||
            trackedImage.trackingState == TrackingState.None)
        {
            _arObjects[trackedImage.referenceImage.name].SetActive(false);
            return;
        }

        var arObject = _arObjects[trackedImage.referenceImage.name];

        arObject.SetActive(true);
        arObject.transform.position = trackedImage.transform.position;
        arObject.transform.rotation = trackedImage.transform.rotation;
    }
}