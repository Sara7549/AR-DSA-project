using UnityEngine;
using TMPro;

public class Bowl : MonoBehaviour
{
    public TextMeshPro numberLabel;
    public Renderer bowlRenderer;
    public Material redMaterial;
    public Material blueMaterial;

    private Plate plateData;

    private void Awake()
    {
        // Auto find components if not assigned
        if (numberLabel == null)
            numberLabel = GetComponentInChildren<TextMeshPro>();
        if (bowlRenderer == null)
            bowlRenderer = GetComponentInChildren<Renderer>();
    }

    public void SetPlate(Plate plate)
    {
        plateData = plate;

        // Set number
        if (numberLabel != null)
            numberLabel.text = plate.number.ToString();

        // Set colour
        if (bowlRenderer != null)
        {
            if (plate.colour == PlateColour.Red)
                bowlRenderer.material = redMaterial;
            else
                bowlRenderer.material = blueMaterial;
        }
    }

    public Plate GetPlate()
    {
        return plateData;
    }
}