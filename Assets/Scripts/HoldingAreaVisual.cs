using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class HoldingAreaVisual : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject hatchbackPrefab;
    public GameObject taxiPrefab;
    public GameObject policePrefab;
    public GameObject pickupPrefab;
    public GameObject truckPrefab;
    public GameObject vanBigPrefab;

    [Header("Slots")]
    public Transform[] slots = new Transform[2];

    [Header("Label")]
    public TextMeshPro areaLabel; // assign in Inspector

    private List<GameObject> vehicleObjects =
        new List<GameObject>();

    private float vehicleScale = 0.02f;

    private void Awake()
    {
        BoxCollider zone = GetComponent<BoxCollider>();
        if (zone == null)
            zone = gameObject.AddComponent<BoxCollider>();

        zone.isTrigger = true;
        zone.size = new Vector3(0.3f, 0.05f, 0.15f);
        zone.center = Vector3.zero;
    }

    private void Start()
    {
        if (areaLabel != null)
            areaLabel.text = "Holding Area";
    }

    private void Update()
    {
        if (areaLabel != null && Camera.main != null)
        {
            areaLabel.transform.LookAt(Camera.main.transform);
            areaLabel.transform.Rotate(0, 180f, 0);
        }
    }

    public void RenderHolding(HoldingArea holding)
    {
        foreach (GameObject v in vehicleObjects)
            Destroy(v);
        vehicleObjects.Clear();

        for (int i = 0; i < holding.vehicles.Count; i++)
        {
            if (i >= slots.Length) break;

            Vehicle vehicle = holding.vehicles[i];
            GameObject prefab = GetPrefab(vehicle);
            if (prefab == null) continue;

            GameObject vehicleObj = Instantiate(prefab,
                slots[i].position,
                transform.rotation);

            vehicleObj.transform.localScale =
                Vector3.one * vehicleScale;

            if (vehicleObj.GetComponentInChildren<Collider>() == null)
            {
                Renderer[] renderers =
                    vehicleObj.GetComponentsInChildren<Renderer>();

                if (renderers.Length > 0)
                {
                    Bounds worldBounds = renderers[0].bounds;
                    foreach (Renderer r in renderers)
                        worldBounds.Encapsulate(r.bounds);

                    BoxCollider col = vehicleObj.AddComponent<BoxCollider>();
                    col.center = vehicleObj.transform
                        .InverseTransformPoint(worldBounds.center);
                    col.size = vehicleObj.transform
                        .InverseTransformVector(worldBounds.size);
                    col.size = new Vector3(
                        Mathf.Abs(col.size.x),
                        Mathf.Abs(col.size.y),
                        Mathf.Abs(col.size.z));
                }
                else
                {
                    BoxCollider col = vehicleObj.AddComponent<BoxCollider>();
                    col.size = new Vector3(1f, 1f, 1f);
                    col.center = new Vector3(0, 0.5f, 0);
                }
            }

            VehicleVisual vv =
                vehicleObj.AddComponent<VehicleVisual>();
            vv.vehicle = vehicle;
            vv.laneIndex = -1;
            vv.slotIndex = i;
            vv.SetOriginalPosition(slots[i].position);

            vehicleObjects.Add(vehicleObj);
        }
    }

    private GameObject GetPrefab(Vehicle vehicle)
    {
        switch (vehicle.prefabType)
        {
            case VehiclePrefabType.Hatchback: return hatchbackPrefab;
            case VehiclePrefabType.Taxi: return taxiPrefab;
            case VehiclePrefabType.Police: return policePrefab;
            case VehiclePrefabType.Pickup: return pickupPrefab;
            case VehiclePrefabType.Truck: return truckPrefab;
            case VehiclePrefabType.VanBig: return vanBigPrefab;
            default: return taxiPrefab;
        }
    }
}