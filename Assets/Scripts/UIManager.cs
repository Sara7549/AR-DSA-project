using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button pushButton;
    public Button popButton;
    private StackManager stackManager;

    private void Update()
    {
        if (stackManager != null)
        {
            bool markerVisible = stackManager.gameObject.activeSelf;
            pushButton.interactable = markerVisible;
            popButton.interactable = markerVisible;
        }
        // Keep trying to find StackManager until found
        if (stackManager == null)
        {
            stackManager = FindObjectOfType<StackManager>();
            if (stackManager != null)
            {
                // Connect buttons once StackManager is found
                pushButton.onClick.AddListener(stackManager.Push);
                popButton.onClick.AddListener(stackManager.Pop);
                Debug.Log("StackManager found and buttons connected");
            }
        }
    }
}