[System.Serializable]
public class Plate
{
    public int id; // 1 to 6, each maps to unique colour

    public Plate(int id)
    {
        this.id = id;
    }

    public override string ToString()
    {
        return "Bowl " + id;
    }
}