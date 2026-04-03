using UnityEngine;
using TMPro;

public class Bowl : MonoBehaviour
{
    public TextMeshPro numberLabel;
    public Renderer bowlRenderer;

    // Assign these in inspector on the bowl prefab
    public Material[] bowlMaterials = new Material[6];

    private Plate plateData;
    private Material originalMaterial;

    private void Awake()
    {
        if (numberLabel == null)
            numberLabel = GetComponentInChildren<TextMeshPro>();

        if (bowlRenderer == null)
        {
            Renderer[] renderers =
                GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                if (r.GetComponent<TextMeshPro>() == null)
                {
                    bowlRenderer = r;
                    break;
                }
            }
        }
    }

    public void SetPlate(Plate plate)
    {
        plateData = plate;

        if (bowlRenderer != null &&
            plate.id >= 1 &&
            plate.id <= bowlMaterials.Length &&
            bowlMaterials[plate.id - 1] != null)
        {
            // Create instance so each bowl has its own material
            originalMaterial = new Material(bowlMaterials[plate.id - 1]);
            bowlRenderer.material = originalMaterial;
        }

        if (numberLabel != null)
            numberLabel.gameObject.SetActive(false);
    }
    public void SetHighlight(bool highlighted)
    {
        if (bowlRenderer == null) return;

        if (highlighted)
        {
            // Create a new material instance so we don't affect shared material
            Material highlightMat = new Material(bowlRenderer.material);
            highlightMat.color = Color.white;
            // Also try URP specific property
            if (highlightMat.HasProperty("_BaseColor"))
                highlightMat.SetColor("_BaseColor", Color.white);
            bowlRenderer.material = highlightMat;
        }
        else if (originalMaterial != null)
        {
            bowlRenderer.material = originalMaterial;
        }
    }

    public Plate GetPlate() => plateData;
}