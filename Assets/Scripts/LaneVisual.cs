using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LaneVisual : MonoBehaviour
{
    [Header("Settings")]
    public int laneIndex;
    public float slotSize = 0.08f;

    [Header("Prefabs")]
    public GameObject hatchbackPrefab;
    public GameObject taxiPrefab;
    public GameObject policePrefab;
    public GameObject vanBigPrefab;
    public GameObject truckPrefab;
    public GameObject pickupPrefab;

    [Header("Slot Transforms")]
    public Transform[] slots = new Transform[5];

    [Header("Target Indicator")]
    public GameObject targetIndicatorPrefab;

    private List<GameObject> vehicleObjects =
        new List<GameObject>();

    private float vehicleScale = 0.03f;
    public float vanBigScale = 0.030001f;

    private void Awake()
    {
        BoxCollider zone = GetComponent<BoxCollider>();
        if (zone == null)
            zone = gameObject.AddComponent<BoxCollider>();

        zone.isTrigger = true;
        zone.size = new Vector3(0.15f, 0.05f, 0.5f);
        zone.center = Vector3.zero;
    }


    public void RenderLane(QueueLane lane)
    {
        // Clear existing vehicles
        foreach (GameObject v in vehicleObjects)
            Destroy(v);
        vehicleObjects.Clear();

        int currentSlot = 0;
        for (int i = 0; i < lane.vehicles.Count; i++)
        {
            Vehicle vehicle = lane.vehicles[i];

            if (currentSlot >= slots.Length) break;

            GameObject prefab = GetPrefabForVehicle(vehicle);
            if (prefab == null)
            {
                currentSlot += vehicle.SlotSize;
                continue;
            }

            // Calculate spawn position
            // For multi-slot vehicles average across occupied slots
            Vector3 spawnPos = slots[currentSlot].position;
            if (vehicle.SlotSize > 1 &&
                currentSlot + vehicle.SlotSize - 1 < slots.Length)
            {
                Vector3 endSlotPos =
                    slots[currentSlot + vehicle.SlotSize - 1].position;
                spawnPos = (spawnPos + endSlotPos) / 2f;
            }

            GameObject vehicleObj = Instantiate(prefab,
                spawnPos,
                transform.rotation);

            float scale = vehicle.prefabType == VehiclePrefabType.VanBig
                 ? vanBigScale
                 : vehicleScale;
            vehicleObj.transform.localScale = Vector3.one * scale;

            // --- CENTERING FIX ---
            // Wait one frame would be ideal but we can use bounds immediately
            Renderer[] renderers =
                vehicleObj.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                foreach (Renderer r in renderers)
                    bounds.Encapsulate(r.bounds);

                // Offset to center vehicle on slot
                Vector3 offset = vehicleObj.transform.position
                    - bounds.center;
                vehicleObj.transform.position = spawnPos +
                    new Vector3(offset.x, 0, offset.z);
            }
            // --- END CENTERING FIX ---

            // Add VehicleVisual component
            VehicleVisual vv =
                vehicleObj.AddComponent<VehicleVisual>();
            vv.vehicle = vehicle;
            vv.laneIndex = laneIndex;
            vv.slotIndex = currentSlot;
            vv.SetOriginalPosition(vehicleObj.transform.position);

            // Add target indicator if this is target vehicle
            if (vehicle.isTarget)
                AddTargetIndicator(vehicleObj);

            vehicleObjects.Add(vehicleObj);
            currentSlot += vehicle.SlotSize;

            // Add collider for drag detection
            // Replace the collider block in both LaneVisual and HoldingAreaVisual
            // Add collider for drag detection
            if (vehicleObj.GetComponentInChildren<Collider>() == null)
            {
                // reuse 'renderers' already declared above in this loop iteration
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
        }
    }

    private int GetSlotIndex(QueueLane lane,
        int vehicleIndex)
    {
        // Calculate slot index based on vehicle sizes
        int slot = 0;
        for (int i = 0; i < vehicleIndex; i++)
            slot += lane.vehicles[i].SlotSize;
        return slot;
    }

    private void AddTargetIndicator(GameObject vehicleObj)
    {
        // Create a simple star/arrow above target vehicle
        GameObject indicator = GameObject.CreatePrimitive(
            PrimitiveType.Sphere);
        indicator.transform.SetParent(vehicleObj.transform);
        indicator.transform.localPosition =
            new Vector3(0, 300f, 0);
        indicator.transform.localScale =
            new Vector3(50f, 50f, 50f);

        // Give it a bright yellow material
        Renderer r = indicator.GetComponent<Renderer>();
        if (r != null)
        {
            Material mat = new Material(
                Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = Color.yellow;
            r.material = mat;
        }

        // Remove collider so it does not interfere
        Destroy(indicator.GetComponent<Collider>());
    }

    private GameObject GetPrefabForVehicle(Vehicle vehicle)
    {
        switch (vehicle.prefabType)
        {
            case VehiclePrefabType.Hatchback:
                return hatchbackPrefab;
            case VehiclePrefabType.Taxi:
                return taxiPrefab;
            case VehiclePrefabType.Police:
                return policePrefab;
            case VehiclePrefabType.Pickup:
                return pickupPrefab;
            case VehiclePrefabType.Truck:
                return truckPrefab;
            case VehiclePrefabType.VanBig:
                return vanBigPrefab;
            default:
                return taxiPrefab;
        }
    }

    public List<GameObject> GetVehicleObjects()
    {
        return vehicleObjects;
    }

    public void SetVehicleScale(float scale)
    {
        vehicleScale = scale;
    }

    public void SlideRemainingForward(int removedSlotIndex, int removedSlotSize)
    {
        int slot = 0;
        foreach (GameObject obj in vehicleObjects)
        {
            VehicleVisual vv = obj.GetComponent<VehicleVisual>();
            if (vv == null) continue;
            if (vv.IsDragging()) continue;

            if (slot < slots.Length)
                vv.SetTargetPosition(
                    GetCenteredSlotPosition(slot, vv.vehicle.SlotSize));

            slot += vv.vehicle.SlotSize;
            if (slot >= slots.Length) break;
        }
    }
    // Call this to restore positions when drag is cancelled
    public void RestorePositions()
    {
        int slot = 0;
        foreach (GameObject obj in vehicleObjects)
        {
            VehicleVisual vv = obj.GetComponent<VehicleVisual>();
            if (vv == null) continue;

            if (slot < slots.Length)
                vv.SetTargetPosition(
                    GetCenteredSlotPosition(slot, vv.vehicle.SlotSize));

            slot += vv.vehicle.SlotSize;
            if (slot >= slots.Length) break;
        }
    }
    // Add this new helper method
    private Vector3 GetCenteredSlotPosition(int startSlot, int slotSize)
    {
        Vector3 pos = slots[startSlot].position;
        int endSlot = startSlot + slotSize - 1;
        if (slotSize > 1 && endSlot < slots.Length)
            pos = (slots[startSlot].position
                   + slots[endSlot].position) / 2f;
        return pos;
    }
}