using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QueueUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI instructionText;
    public GameObject winPanel;
    public TextMeshProUGUI winText;
    public TextMeshProUGUI moveCountText;
    public Button restartButton;
    public Button backButton;

    private bool isPlaced = false;

    private void Start()
    {
        if (instructionText != null)
            instructionText.text = "";

        if (winPanel != null)
            winPanel.SetActive(false);

        Invoke("ShowInitialInstruction", 2f);
    }

    private void ShowInitialInstruction()
    {
        if (!isPlaced && instructionText != null)
            instructionText.text =
                "Move your phone slowly to detect a surface";
    }

    public void SetStateScan()
    {
        if (!isPlaced && instructionText != null)
            instructionText.text =
                "Move your phone slowly to detect a surface";
    }

    public void SetStatePlace()
    {
        if (!isPlaced && instructionText != null)
            instructionText.text = "Tap to place the parking lot";
    }

    public void SetStateDrag()
    {
        if (instructionText != null)
            instructionText.text =
                "Drag a vehicle to move it";
    }

    public void OnGamePlaced(GameObject queueGroup)
    {
        isPlaced = true;

        if (instructionText != null)
            instructionText.text =
                "Get the target cars to the exit zone!";

        Invoke("SetStateDrag", 3f);

        StartCoroutine(PlacementAnimation(queueGroup));
    }

    public void UpdateMoveCount(int moves)
    {
        if (moveCountText != null)
            moveCountText.text = "Moves: " + moves;
    }

    public void ShowWinScreen(int moves)
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            if (winText != null)
                winText.text = "You did it in " +
                    moves + " moves!\n\n" +
                    GetPerformanceMessage(moves);
        }
    }

    private string GetPerformanceMessage(int moves)
    {
        if (moves <= 5) return "Outstanding!";
        else if (moves <= 10) return "Great job!";
        else if (moves <= 15) return "Well done!";
        else return "Keep practicing!";
    }

    public void ShowFeedback(string message)
    {
        StartCoroutine(ShowFeedbackCoroutine(message));
    }

    private IEnumerator ShowFeedbackCoroutine(string message)
    {
        if (instructionText == null) yield break;

        Color originalColor = instructionText.color;
        string originalText = instructionText.text;

        instructionText.text = message;
        instructionText.color = Color.red;

        yield return new WaitForSeconds(1.5f);

        instructionText.text = originalText;
        instructionText.color = originalColor;
    }

    private IEnumerator PlacementAnimation(GameObject obj)
    {
        if (obj == null) yield break;

        Vector3 targetScale = obj.transform.localScale;
        if (targetScale == Vector3.zero)
            targetScale = Vector3.one;

        obj.transform.localScale = Vector3.zero;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            obj.transform.localScale =
                Vector3.Lerp(Vector3.zero, targetScale, t);
            yield return null;
        }

        obj.transform.localScale = targetScale;
    }

    public void HideWinPanel()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
    }
}