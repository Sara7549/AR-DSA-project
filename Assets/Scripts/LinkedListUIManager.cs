using UnityEngine;
using TMPro;
using System.Collections;

public class LinkedListUIManager : MonoBehaviour
{
    [Header("AR UI")]
    public TextMeshProUGUI instructionText;
    public GameObject reticle;

    private bool isPlaced = false;
    private Color originalColor;

    private enum TutorialState
    {
        Scan,
        Place,
        DragPointer,
        ConnectNodes,
        Garbage,
        Complete
    }

    private TutorialState currentState;

    private void Start()
    {
        currentState = TutorialState.Scan;

        if (instructionText != null)
            originalColor = instructionText.color;

        UpdateInstruction();
    }

    void UpdateInstruction()
    {
        if (instructionText == null) return;

        instructionText.color = originalColor;

        switch (currentState)
        {
            case TutorialState.Scan:
                instructionText.text = "Move your phone backwards slowly to detect a surface";
                break;

            case TutorialState.Place:
                instructionText.text = "Tap to place the train track";
                break;

            case TutorialState.DragPointer:
                instructionText.text = "Drag the pointer to another train car";
                break;

            case TutorialState.ConnectNodes:
                instructionText.text = "Connect nodes to match the target order";
                break;

            case TutorialState.Garbage:
                instructionText.text = "Disconnected cars are garbage collected!";
                break;

            case TutorialState.Complete:
                instructionText.text = "Well done! List completed!";
                break;
        }
    }

    // Called after placement
    public void OnListPlaced(GameObject listGroup)
    {
        isPlaced = true;

        if (reticle != null)
            reticle.SetActive(false);

        currentState = TutorialState.DragPointer;
        UpdateInstruction();

        StartCoroutine(PlacementAnimation(listGroup));
    }

    public void SetStatePlace()
    {
        currentState = TutorialState.Place;
        UpdateInstruction();
    }

    public void SetStateScan()
    {
        currentState = TutorialState.Scan;
        UpdateInstruction();
    }

    public void SetStateDrag()
    {
        currentState = TutorialState.DragPointer;
        UpdateInstruction();
    }

    public void SetStateConnect()
    {
        currentState = TutorialState.ConnectNodes;
        UpdateInstruction();
    }

    public void SetStateGarbage()
    {
        currentState = TutorialState.Garbage;
        UpdateInstruction();
    }

    public void SetStateComplete()
    {
        currentState = TutorialState.Complete;
        UpdateInstruction();
    }

    // Feedback system (important for learning)
    private Coroutine feedbackCoroutine;

    public void ShowFeedback(string message, Color color)
    {
        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);

        feedbackCoroutine = StartCoroutine(FlashFeedback(message, color));
    }

    private IEnumerator FlashFeedback(string message, Color color)
    {
        if (instructionText == null) yield break;

        instructionText.text = message;
        instructionText.color = color;

        yield return new WaitForSeconds(1.5f);

        instructionText.color = originalColor;
        UpdateInstruction();
    }

    // Placement animation (same as your stack but reusable)
    private IEnumerator PlacementAnimation(GameObject obj)
    {
        if (obj == null) yield break;

        Vector3 targetScale = obj.transform.localScale;

        if (targetScale == Vector3.zero)
            targetScale = Vector3.one;

        obj.transform.localScale = Vector3.zero;

        float duration = 0.5f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = t / duration;
            lerp = lerp * lerp * (3f - 2f * lerp);

            obj.transform.localScale =
                Vector3.Lerp(Vector3.zero, targetScale, lerp);

            yield return null;
        }

        obj.transform.localScale = targetScale;
    }
    [Header("Target Display")]
    public TextMeshProUGUI targetOrderText;

    public void ShowTargetOrder(string orderText)
    {
        if (targetOrderText != null)
            targetOrderText.text = orderText;
    }
}