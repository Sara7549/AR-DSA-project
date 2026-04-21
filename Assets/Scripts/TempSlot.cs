using UnityEngine;
using TMPro;

public class TempSlot : MonoBehaviour
{
    public static TempSlot Instance;

    // The node currently saved in temp
    public Node savedNode { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI tempLabel;
    public GameObject tempIndicator; // a UI panel showing temp is occupied

    private void Awake()
    {
        Instance = this;
        UpdateUI();
    }

    public void Save(Node node)
    {
        savedNode = node;
        UpdateUI();
    }

    public void Clear()
    {
        savedNode = null;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (tempLabel != null)
        {
            if (savedNode != null)
                tempLabel.text = "temp = Carriage_" + savedNode.value;
            else
                tempLabel.text = "temp = null";
        }

        if (tempIndicator != null)
            tempIndicator.SetActive(savedNode != null);
    }

    // Called when player drags onto the temp slot UI area
    public void OnNodeDropped(Node node)
    {
        Save(node);
    }
}