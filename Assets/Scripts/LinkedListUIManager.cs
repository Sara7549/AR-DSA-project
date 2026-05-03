using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class LinkedListUIManager : MonoBehaviour
{
    [Header("AR UI")]
    public TextMeshProUGUI instructionText;
    public GameObject reticle;

    private bool isPlaced = false;
    private Color originalColor;


    [Header("Win Screen")]
    public GameObject winPanel;
    public TextMeshProUGUI winText;

    [Header("Move Counter")]
    public TextMeshProUGUI moveCountText;

    public void UpdateMoveCount(int count)
    {
        if (moveCountText != null)
            moveCountText.text = "Moves: " + count;
    }
    [Header("Undo Button")]
    public GameObject undoButtonObject;  // for SetActive
    public Button undoButton;            // for onClick

    [Header("Instructions Panel")]
    public GameObject instructionsPanel;
    public GameObject instructionIcon;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private bool instructionMinimized = false;

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

        if (undoButtonObject != null)
            undoButtonObject.SetActive(false);

        if (undoButton != null)
            undoButton.onClick.AddListener(OnUndoPressed);

        if (instructionsPanel != null)
        {
            RectTransform rect =
                instructionsPanel.GetComponent<RectTransform>();
            originalScale = rect.localScale;
            originalPosition = rect.anchoredPosition;
            instructionsPanel.SetActive(false);
        }

        if (instructionIcon != null)
            instructionIcon.SetActive(false);

        UpdateInstruction();
    }

    private void OnUndoPressed()
    {
        LinkedListGameManager gm =
            FindObjectOfType<LinkedListGameManager>();
        if (gm != null)
            gm.UndoLastMove();
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
                instructionText.text = "Drag the connector from one train car to the other";
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
        if (instructionsPanel != null)
        {
            RectTransform rect =
                instructionsPanel.GetComponent<RectTransform>();
            originalScale = rect.localScale;
            originalPosition = rect.anchoredPosition;
            instructionsPanel.SetActive(true);
            if (instructionIcon != null)
                instructionIcon.SetActive(true);
            if (undoButtonObject != null)
                undoButtonObject.SetActive(true);
        }

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

    public void SetStateComplete(int moves)
    {
        currentState = TutorialState.Complete;
        UpdateInstruction();

        if (winPanel != null)
        {
            winPanel.SetActive(true);

            if (winText != null)
                winText.text = "You sorted the train in " + moves + " moves!\n\n"
                             + GetPerformanceMessage(moves);
        }
    }

    private string GetPerformanceMessage(int moves)
    {
        if (moves <= 3) return "Outstanding!";
        else if (moves <= 6) return "Great job!";
        else if (moves <= 10) return "Well done!";
        else return "Keep practicing!";
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
    
    public void ShowDebug(string message)
    {
        if (instructionText != null)
            instructionText.text = message;
    }

    public void MinimizeInstructions()
    {
        if (instructionsPanel == null) return;
        StartCoroutine(AnimateInstructionMinimize());
        instructionMinimized = true;
    }

    private IEnumerator AnimateInstructionMinimize()
    {
        RectTransform panelRect =
            instructionsPanel.GetComponent<RectTransform>();
        RectTransform iconRect =
            instructionIcon.GetComponent<RectTransform>();

        float duration = 0.35f;
        float elapsed = 0f;

        Vector3 startScale = panelRect.localScale;
        Vector3 startPos = panelRect.anchoredPosition;
        Vector3 endScale = new Vector3(0.2f, 0.2f, 0.2f);
        Vector3 endPos = iconRect.anchoredPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float smoothT = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            panelRect.localScale =
                Vector3.Lerp(startScale, endScale, smoothT);
            panelRect.anchoredPosition =
                Vector3.Lerp(startPos, endPos, smoothT);
            yield return null;
        }

        panelRect.localScale = endScale;
        panelRect.anchoredPosition = endPos;
        instructionsPanel.SetActive(false);
        instructionIcon.SetActive(true);
    }

    public void ShowInstructions()
    {
        RectTransform rect =
            instructionsPanel.GetComponent<RectTransform>();
        rect.localScale = originalScale;
        rect.anchoredPosition = originalPosition;
        instructionsPanel.SetActive(true);
        instructionMinimized = false;
    }

    public void ToggleInstructions()
    {
        if (instructionsPanel.activeSelf)
            MinimizeInstructions();
        else
            ShowInstructions();
    }
}