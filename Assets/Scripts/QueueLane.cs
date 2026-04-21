using System.Collections.Generic;
using UnityEngine;

public class QueueLane
{
    public List<Vehicle> vehicles = new List<Vehicle>();
    public int maxSlots = 5;

    public int UsedSlots
    {
        get
        {
            int total = 0;
            foreach (Vehicle v in vehicles)
                total += v.SlotSize;
            return total;
        }
    }

    public int FreeSlots => maxSlots - UsedSlots;

    public bool IsEmpty => vehicles.Count == 0;

    public Vehicle Front =>
        IsEmpty ? null : vehicles[0];

    public Vehicle Back =>
        IsEmpty ? null : vehicles[vehicles.Count - 1];

    // Check if vehicle can be added to back of lane
    public bool CanEnqueue(Vehicle vehicle)
    {
        return FreeSlots >= vehicle.SlotSize;
    }

    // Remove from front of queue
    public Vehicle Dequeue()
    {
        if (IsEmpty) return null;
        Vehicle front = vehicles[0];
        vehicles.RemoveAt(0);
        return front;
    }

    // Add to back of queue
    public bool Enqueue(Vehicle vehicle)
    {
        if (!CanEnqueue(vehicle)) return false;
        vehicles.Add(vehicle);
        return true;
    }

    // Check if target car is at front
    public bool IsTargetAtFront()
    {
        return !IsEmpty && Front.isTarget;
    }

    public bool HasTarget()
    {
        foreach (Vehicle v in vehicles)
            if (v.isTarget) return true;
        return false;
    }
}