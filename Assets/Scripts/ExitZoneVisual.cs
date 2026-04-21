using UnityEngine;
using System.Collections.Generic;

public class ExitZoneVisual : MonoBehaviour
{
    [Header("Slots")]
    public Transform[] exitSlots = new Transform[3];

    private List<GameObject> exitedVehicles =
        new List<GameObject>();

    private void Awake()
    {
        BoxCollider zone = GetComponent<BoxCollider>();
        if (zone == null)
            zone = gameObject.AddComponent<BoxCollider>();

        zone.isTrigger = true;
        zone.size = new Vector3(0.15f, 0.05f, 0.15f);
        zone.center = Vector3.zero;
    }

    public void AddExitedVehicle(GameObject vehicleObj,
        int slotIndex)
    {
        if (slotIndex >= exitSlots.Length) return;

        vehicleObj.transform.position =
            exitSlots[slotIndex].position;
        vehicleObj.transform.SetParent(transform);
        exitedVehicles.Add(vehicleObj);
    }

    public void Clear()
    {
        foreach (GameObject v in exitedVehicles)
            Destroy(v);
        exitedVehicles.Clear();
    }
}