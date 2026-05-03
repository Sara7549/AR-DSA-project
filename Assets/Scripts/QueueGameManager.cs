using System.Collections.Generic;
using UnityEngine;

public class QueueGameManager : MonoBehaviour
{
    public static QueueGameManager Instance;

    public QueueLane[] lanes = new QueueLane[3];
    public HoldingArea holdingArea = new HoldingArea();
    public List<Vehicle> exitedVehicles = new List<Vehicle>();

    public int moveCount = 0;
    private bool gameWon = false;

    private void Awake()
    {
        Instance = this;
    }

    public void InitializeGame()
    {
        lanes = new QueueLane[3];
        for (int i = 0; i < 3; i++)
            lanes[i] = new QueueLane();

        holdingArea = new HoldingArea();
        exitedVehicles.Clear();
        moveCount = 0;
        gameWon = false;

        GenerateRandomLevel();
    }

    private void GenerateRandomLevel()
    {
        System.Random rng = new System.Random();

        VehiclePrefabType[] oneSlotTypes = new VehiclePrefabType[]
        {
        VehiclePrefabType.Taxi,
        VehiclePrefabType.Police,
        VehiclePrefabType.Pickup
        };

        // Decide upfront how many lanes will have the target buried (not at front).
        // We want AT LEAST 2 targets buried, so at most 1 can be at the front.
        // Shuffle lane indices and mark the first 2 as "must be buried".
        int[] laneOrder = new int[] { 0, 1, 2 };
        // Fisher-Yates shuffle
        for (int i = laneOrder.Length - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            int tmp = laneOrder[i];
            laneOrder[i] = laneOrder[j];
            laneOrder[j] = tmp;
        }
        // First 2 lanes in the shuffled order must have buried targets
        HashSet<int> mustBeBuried = new HashSet<int>
        {
            laneOrder[0], laneOrder[1]
        };

        bool vanPlaced = false;

        for (int laneIndex = 0; laneIndex < 3; laneIndex++)
        {
            List<Vehicle> laneVehicles = new List<Vehicle>();
            int remainingSlots = 5;
            bool targetPlaced = false;
            int truckCount = 0;
            int maxTrucks = 1; // max one truck per lane

            // For buried lanes, block "target" from being placed until at least
            // one non-target vehicle has already been added (occupying >= 1 slot).
            bool requireBuried = mustBeBuried.Contains(laneIndex);


            while (remainingSlots > 0)
            {
                // Force target if it is the last slot
                if (remainingSlots == 1 && !targetPlaced)
                {
                    laneVehicles.Add(new Vehicle(
                        VehicleType.Car,
                        VehiclePrefabType.Hatchback,
                        laneIndex, true));
                    targetPlaced = true;
                    remainingSlots -= 1;
                    continue;
                }

                int slotsUsed = 5 - remainingSlots;

                // Build a list of valid options
                // given current remaining slots
                List<string> options = new List<string>();

                // Always can place 1-slot car
                options.Add("car");
                options.Add("car"); // weighted higher

                // Target if not placed yet
                if (!targetPlaced &&
                   (!requireBuried || slotsUsed >= 1))
                {
                    options.Add("target");
                }

                // Truck if enough slots and under limit
                if (remainingSlots >= 2 &&
                    truckCount < maxTrucks)
                {
                    options.Add("truck");
                }

                // VanBig if enough slots
                // but only if no truck placed yet
                // to avoid too many big vehicles
                if (remainingSlots >= 3 && truckCount == 0 && !vanPlaced)
                {
                    options.Add("vanbig");
                }

                // Pick random valid option
                string choice =
                    options[rng.Next(0, options.Count)];

                switch (choice)
                {
                    case "target":
                        laneVehicles.Add(new Vehicle(
                            VehicleType.Car,
                            VehiclePrefabType.Hatchback,
                            laneIndex, true));
                        targetPlaced = true;
                        remainingSlots -= 1;
                        break;

                    case "truck":
                        laneVehicles.Add(new Vehicle(
                            VehicleType.Truck,
                            VehiclePrefabType.Truck,
                            rng.Next(10, 99)));
                        truckCount++;
                        remainingSlots -= 2;
                        break;

                    case "vanbig":
                        laneVehicles.Add(new Vehicle(
                            VehicleType.VanBig,
                            VehiclePrefabType.VanBig,
                            rng.Next(10, 99)));
                        truckCount++; // counts toward big vehicle limit
                        remainingSlots -= 3;
                        vanPlaced = true;
                        break;

                    case "car":
                    default:
                        VehiclePrefabType t =
                            oneSlotTypes[rng.Next(0,
                                oneSlotTypes.Length)];
                        laneVehicles.Add(new Vehicle(
                            VehicleType.Car,
                            t,
                            rng.Next(10, 99)));
                        remainingSlots -= 1;
                        break;
                }
            }

            // Safety check — if target never placed
            // replace a random non-target car with target
            if (!targetPlaced)
            {
                for (int i = 0; i < laneVehicles.Count; i++)
                {
                    if (laneVehicles[i].type == VehicleType.Car
                        && !laneVehicles[i].isTarget)
                    {
                        laneVehicles[i] = new Vehicle(
                            VehicleType.Car,
                            VehiclePrefabType.Hatchback,
                            laneIndex, true);
                        break;
                    }
                }
            }

            foreach (Vehicle v in laneVehicles)
                lanes[laneIndex].Enqueue(v);
        }
    }

    private int GetTotalSlots(List<Vehicle> vehicles)
    {
        int total = 0;
        foreach (Vehicle v in vehicles)
            total += v.SlotSize;
        return total;
    }

    public bool MoveToHolding(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= lanes.Length)
            return false;

        Vehicle front = lanes[laneIndex].Front;
        if (front == null) return false;

        if (front.isTarget)
            return false;

        if (!holdingArea.CanHold(front))
            return false;

        lanes[laneIndex].Dequeue();
        holdingArea.Add(front);
        moveCount++;
        return true;
    }

    public bool MoveFromHolding(Vehicle vehicle, int laneIndex)
    {
        if (!holdingArea.vehicles.Contains(vehicle))
            return false;

        if (!lanes[laneIndex].CanEnqueue(vehicle))
            return false;

        holdingArea.Remove(vehicle);
        lanes[laneIndex].Enqueue(vehicle);
        moveCount++;
        return true;
    }

    public bool MoveBetweenLanes(int fromLane, int toLane)
    {
        if (fromLane == toLane) return false;

        Vehicle front = lanes[fromLane].Front;
        if (front == null) return false;

        if (!lanes[toLane].CanEnqueue(front))
            return false;

        lanes[fromLane].Dequeue();
        lanes[toLane].Enqueue(front);
        moveCount++;
        return true;
    }

    public bool TryExitTarget(int laneIndex)
    {
        if (!lanes[laneIndex].IsTargetAtFront())
            return false;

        Vehicle target = lanes[laneIndex].Dequeue();
        exitedVehicles.Add(target);
        moveCount++;

        CheckWin();
        return true;
    }

    private void CheckWin()
    {
        // Win when all 3 targets exited
        int targetCount = 0;
        foreach (Vehicle v in exitedVehicles)
            if (v.isTarget) targetCount++;

        if (targetCount >= 3)
        {
            gameWon = true;
        }
    }

    public bool IsGameWon() => gameWon;

    public int GetSelectedStack() => -1;

    public void RestartGame()
    {
        moveCount = 0;
        gameWon = false;
        InitializeGame();
    }
    // Call when drag starts — removes vehicle from lane data temporarily
    public bool TryLiftFromLane(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= lanes.Length) return false;
        Vehicle front = lanes[laneIndex].Front;
        if (front == null) return false;

        lanes[laneIndex].Dequeue();
        return true;
    }

    // Call when drag is cancelled — puts vehicle back at front
    public void ReturnToLane(int laneIndex, Vehicle vehicle)
    {
        if (laneIndex < 0 || laneIndex >= lanes.Length) return;
        // Insert at front of queue
        lanes[laneIndex].vehicles.Insert(0, vehicle);
    }
    // Vehicle already dequeued — just enqueue to destination
    public bool TryEnqueueToLane(Vehicle vehicle, int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= lanes.Length) return false;
        if (!lanes[laneIndex].CanEnqueue(vehicle)) return false;
        lanes[laneIndex].Enqueue(vehicle);
        moveCount++;
        return true;
    }

    // Vehicle already dequeued — just add to holding
    public bool TryAddToHolding(Vehicle vehicle)
    {
        if (!holdingArea.CanHold(vehicle)) return false;
        holdingArea.Add(vehicle);
        moveCount++;
        return true;
    }

    // Vehicle already dequeued — register as exited
    public bool TryExitLifted(Vehicle vehicle)
    {
        if (!vehicle.isTarget) return false;
        exitedVehicles.Add(vehicle);
        moveCount++;
        CheckWin();
        return true;
    }
    // After GenerateRandomLevel(), calculate a fair move limit


private int GetTargetDepth(int laneIndex)
{
    int depth = 0;
    foreach (Vehicle v in lanes[laneIndex].vehicles)
    {
        if (v.isTarget) break;
        depth++;
    }
    return depth;
}
    public bool AreAllTargetsAtFront()
    {
        for (int i = 0; i < lanes.Length; i++)
        {
            if (lanes[i].Front == null) return false;

            if (!lanes[i].Front.isTarget)
                return false;
        }
        return true;
    }
}