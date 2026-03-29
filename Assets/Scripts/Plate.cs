public enum PlateColour { Red, Blue }

[System.Serializable]
public class Plate
{
    public PlateColour colour;
    public int number;

    public Plate(PlateColour colour, int number)
    {
        this.colour = colour;
        this.number = number;
    }

    public override string ToString()
    {
        return colour + " " + number;
    }
}