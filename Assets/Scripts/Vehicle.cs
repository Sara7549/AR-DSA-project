public enum VehicleType
{
    Car,    // 1 slot
    Truck,  // 2 slots
    VanBig  // 3 slots
}

public enum VehiclePrefabType
{
    Hatchback,  // target — always green
    Taxi,       // 1 slot obstacle
    Police,     // 1 slot obstacle
    Pickup,     // 1 slot obstacle
    Truck,      // 2 slot obstacle
    VanBig      // 3 slot obstacle
}

[System.Serializable]
public class Vehicle
{
    public VehicleType type;
    public VehiclePrefabType prefabType;
    public int colourId;
    public bool isTarget;

    public int SlotSize
    {
        get
        {
            switch (type)
            {
                case VehicleType.Truck: return 2;
                case VehicleType.VanBig: return 3;
                default: return 1;
            }
        }
    }

    public Vehicle(VehicleType type,
        VehiclePrefabType prefabType,
        int colourId,
        bool isTarget = false)
    {
        this.type = type;
        this.prefabType = prefabType;
        this.colourId = colourId;
        this.isTarget = isTarget;
    }

    public override string ToString()
    {
        return (isTarget ? "TARGET " : "") +
            prefabType + " (" + type + ")";
    }
}