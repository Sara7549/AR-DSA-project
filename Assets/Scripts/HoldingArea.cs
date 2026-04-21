using System.Collections.Generic;

public class HoldingArea
{
    public List<Vehicle> vehicles = new List<Vehicle>();
    public int maxSlots = 2;

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

    public bool CanHold(Vehicle vehicle)
    {
        // Trucks (3 slots) can never fit in 2 slot holding area
        return FreeSlots >= vehicle.SlotSize;
    }

    public bool Add(Vehicle vehicle)
    {
        if (!CanHold(vehicle)) return false;
        vehicles.Add(vehicle);
        return true;
    }

    public bool Remove(Vehicle vehicle)
    {
        return vehicles.Remove(vehicle);
    }
}