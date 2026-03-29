using System.Collections.Generic;

public class GameStack
{
    public List<Plate> plates = new List<Plate>();
    public int maxSize = 4;

    public bool IsFull()
    {
        return plates.Count >= maxSize;
    }

    public bool IsEmpty()
    {
        return plates.Count == 0;
    }

    public Plate PeekTop()
    {
        if (IsEmpty()) return null;
        return plates[plates.Count - 1];
    }

    public bool Push(Plate plate)
    {
        if (IsFull()) return false;
        plates.Add(plate);
        return true;
    }

    public Plate Pop()
    {
        if (IsEmpty()) return null;
        Plate top = plates[plates.Count - 1];
        plates.RemoveAt(plates.Count - 1);
        return top;
    }

    // No constraint version
    public bool CanPlace(Plate plate)
    {
        return !IsFull();
    }

    public bool IsSorted(PlateColour expectedColour)
    {
        if (plates.Count != 3) return false;
        for (int i = 0; i < plates.Count; i++)
        {
            if (plates[i].colour != expectedColour) return false;
            if (plates[i].number != i + 1) return false;
        }
        return true;
    }
}