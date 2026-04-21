using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("AR UI")]
    public TextMeshProUGUI instructionText;
    public GameObject reticle;

    private bool isPlaced = false;
    public Color originalColor;

    private enum TutorialState
    {
        Scan,
        Place,
        Select,
        Move
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
                instructionText.text = "Move phone backwards slowly until you see a yellow circle to detect a surface";
                break;

            case TutorialState.Place:
                instructionText.text = "Tap on the circle to place the stacks";
                break;

            case TutorialState.Select:
                instructionText.text = "Tap a stack to select it";
                break;

            case TutorialState.Move:
                instructionText.text = "Tap another stack to move the bowl";
                break;
        }
    }

    public void OnStackPlaced(GameObject stackGroup)
    {
        isPlaced = true;

        if (reticle != null)
            reticle.SetActive(false);

        currentState = TutorialState.Select;
        UpdateInstruction();

        GameController gc = FindObjectOfType<GameController>();
        if (gc != null && gc.goalPanel != null)
        {
            gc.goalPanel.SetActive(true);
        }
        
        if (gc != null && gc.instructionsPanel != null)
        {
            gc.instructionsPanel.SetActive(true);
        }
        StartCoroutine(PlacementAnimation(stackGroup));
    }

    public void SetStatePlace()
    {
        currentState = TutorialState.Place;
        UpdateInstruction();
    }

    public void SetStateMove()
    {
        currentState = TutorialState.Move;
        UpdateInstruction();
    }

    private IEnumerator PlacementAnimation(GameObject stackGroup)
    {
        if (stackGroup == null) yield break;

        Vector3 targetScale = stackGroup.transform.localScale;

        if (targetScale == Vector3.zero)
            targetScale = new Vector3(1f, 1f, 1f);

        stackGroup.transform.localScale = Vector3.zero;

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);

            stackGroup.transform.localScale =
                Vector3.Lerp(Vector3.zero, targetScale, t);

            yield return null;
        }

        stackGroup.transform.localScale = targetScale;
    }
    public void SetStateSelect()
    {
        currentState = TutorialState.Select;
        UpdateInstruction();
    }
    public void SetStateScan()
    {
        currentState = TutorialState.Scan;
        UpdateInstruction();
    }
    private Coroutine feedbackCoroutine;

    public void ShowFeedback(string message)
    {
        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(FlashFeedback(message));
    }

    private IEnumerator FlashFeedback(string message)
    {
        if (instructionText == null) yield break;

        instructionText.text = message;
        instructionText.color = Color.red;

        yield return new WaitForSeconds(1.2f);

        instructionText.color = originalColor;
        UpdateInstruction(); // restores the correct state text
    }
}