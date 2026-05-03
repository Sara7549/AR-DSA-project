using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ExitZoneVisual : MonoBehaviour
{
    [Header("Slots")]
    public Transform[] exitSlots = new Transform[3];

    [Header("Label")]
    public TextMeshPro areaLabel;

    private List<GameObject> exitedVehicles =
        new List<GameObject>();

    //  NEW: highlight support
    private Renderer rend;
    private Color originalColor;

    private void Awake()
    {
        BoxCollider zone = GetComponent<BoxCollider>();
        if (zone == null)
            zone = gameObject.AddComponent<BoxCollider>();

        zone.isTrigger = true;
        zone.size = new Vector3(0.15f, 0.05f, 0.15f);
        zone.center = Vector3.zero;

        //  get renderer (important!)
        rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            rend.material = new Material(rend.material); // avoid shared material issue
            originalColor = rend.material.color;
        }
    }

    private void Start()
    {
        if (areaLabel != null)
            areaLabel.text = "Exit Zone";
    }

    private void Update()
    {
        if (areaLabel != null && Camera.main != null)
        {
            areaLabel.transform.LookAt(Camera.main.transform);
            areaLabel.transform.Rotate(0, 180f, 0);
        }
    }

    //  NEW
    public void Highlight(Color color)
    {
        if (rend != null)
            rend.material.color = color;
    }

    //  NEW
    public void ResetHighlight()
    {
        if (rend != null)
            rend.material.color = originalColor;
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

        //  reset highlight on restart
        ResetHighlight();
    }
}