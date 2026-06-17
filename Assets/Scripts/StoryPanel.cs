using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoryPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject storyPanelObject;
   // public TextMeshProUGUI storyIcon;
    public TextMeshProUGUI storyTitle;
    public TextMeshProUGUI storyText;
    public Button dismissButton;
    private ARPlacement arPlacement;
    private QueueARPlacement queuePlacement;
    private ARLinkedListPlacement linkedListPlacement;

    [Header("Story Content")]
    //[TextArea(3, 6)]
    //public string icon = "🍽️";
    [TextArea(1, 2)]
    public string title = "Kitchen Chaos!";
    [TextArea(3, 6)]
    public string story = "The chef has a pile of " +
        "colour-coded bowls that need to be sorted " +
        "before the restaurant opens. Help him arrange " +
        "the bowls into the correct order — but remember," +
        " in a stack you can only take from the top!";

    private void Start()
    {
        arPlacement = FindObjectOfType<ARPlacement>();
        queuePlacement = FindObjectOfType<QueueARPlacement>();
        linkedListPlacement = FindObjectOfType<ARLinkedListPlacement>();

        SetPlacementLocked(true);

        // Show story panel at start
        if (storyPanelObject != null)
            storyPanelObject.SetActive(true);

        // Set content
       // if (storyIcon != null) storyIcon.text = icon;
        if (storyTitle != null) storyTitle.text = title;
        if (storyText != null) storyText.text = story;

        // Connect dismiss button
        if (dismissButton != null)
            dismissButton.onClick.AddListener(DismissStory);

        // Auto dismiss after 15 seconds if player does not tap
        Invoke("DismissStory", 15f);
    }

    public void DismissStory()
    {
        if (storyPanelObject != null)
            storyPanelObject.SetActive(false);

        SetPlacementLocked(false);

        // Cancel auto dismiss if player tapped manually
        CancelInvoke("DismissStory");
    }

    public void ShowStory()
    {
        if (storyPanelObject != null)
            storyPanelObject.SetActive(true);
    }

    private void SetPlacementLocked(bool locked)
    {
        if (arPlacement != null)
            arPlacement.SetLocked(locked);
        if (queuePlacement != null)
            queuePlacement.SetLocked(locked);
        if (linkedListPlacement != null)
            linkedListPlacement.SetLocked(locked);
    }
}