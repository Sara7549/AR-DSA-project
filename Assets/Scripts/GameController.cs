using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;

public class GameController : MonoBehaviour
{
    [Header("Stack Visuals")]
    public StackVisual[] stackVisuals = new StackVisual[3];

    [Header("UI")]
    public GameObject winPanel;
    public TextMeshProUGUI moveCountText;
    public TextMeshProUGUI winText;
    public Button restartButton;

    [Header("Goal Display")]
    public TextMeshProUGUI goalText;
    public GameObject goalPanel;

    private GameManager gameManager;
    private bool isAnimating = false;

    public void InitializeGame()
    {
        gameManager = GameManager.Instance;
        gameManager.InitializeGame();

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }

        if (winPanel != null)
            winPanel.SetActive(false);

        if (goalPanel != null)
            goalPanel.SetActive(false);

        RenderAllStacks();
        UpdateMoveCount();
        UpdateGoalDisplay();
    }

    public void OnStackTapped(int stackIndex)
    {
        if (isAnimating) return;

        int previousSelected = gameManager.GetSelectedStack();

        // Check if this would be an invalid move BEFORE telling GameManager
        if (previousSelected != -1 &&
            previousSelected != stackIndex &&
            gameManager.stacks[stackIndex].IsFull())
        {
            // Reject the move and show feedback — don't deselect
            ShowInvalidMoveFeedback("Stack is full!");
            return;
        }

        gameManager.OnStackTapped(stackIndex);
        int newSelected = gameManager.GetSelectedStack();

        UIManager ui = FindObjectOfType<UIManager>();
        if (ui != null)
        {
            if (newSelected == -1)
                ui.SetStateSelect();
            else
                ui.SetStateMove();
        }

        for (int i = 0; i < stackVisuals.Length; i++)
            stackVisuals[i].SetSelected(i == newSelected);

        if (previousSelected != -1 &&
            newSelected == -1 &&
            previousSelected != stackIndex)
        {
            StartCoroutine(AnimateAndRender(previousSelected, stackIndex));
        }

        UpdateMoveCount();
    }

    private IEnumerator AnimateAndRender(int fromIndex, int toIndex)
    {
        isAnimating = true;

        // Sample target height from the updated model (plate is already moved)
        int targetHeight = gameManager.stacks[toIndex].plates.Count;

        yield return StartCoroutine(
            stackVisuals[fromIndex].AnimateMoveTo(
                stackVisuals[toIndex],
                targetHeight));

        // Re-render both stacks — this snaps everything to ground truth
        stackVisuals[fromIndex].RenderStack(gameManager.stacks[fromIndex]);
        stackVisuals[toIndex].RenderStack(gameManager.stacks[toIndex]);

        // Now it's safe to remove the flying bowl — the re-rendered stack
        // is already visible so there's zero gap
        if (stackVisuals[fromIndex]._pendingMovingBowl != null)
        {
            Destroy(stackVisuals[fromIndex]._pendingMovingBowl);
            stackVisuals[fromIndex]._pendingMovingBowl = null;
        }

        yield return new WaitForSeconds(0.05f);

        isAnimating = false;
        CheckWinUI();
    }

    private void RenderAllStacks()
    {
        for (int i = 0; i < stackVisuals.Length; i++)
            stackVisuals[i].RenderStack(gameManager.stacks[i]);
    }

    private void UpdateMoveCount()
    {
        if (moveCountText != null)
            moveCountText.text = "Moves: " +
                gameManager.moveCount;
    }

    public void UpdateGoalDisplay()
    {
        if (goalText == null) return;

        string[] colourSymbols = new string[]
        {
        "<color=#E63333>[R]</color>",
        "<color=#3366E6>[B]</color>",
        "<color=#33CC4D>[G]</color>",
        "<color=#E6CC1A>[Y]</color>",
        "<color=#B233E6>[P]</color>",
        "<color=#E6801A>[O]</color>"
        };

        // Use a table-style layout with fixed column widths
        string display = "TARGET:\n";
        display += " S1     S2     S3\n";
        display += "------------------\n";

        int maxHeight = 0;
        for (int i = 0; i < gameManager.goalStacks.Length; i++)
            if (gameManager.goalStacks[i].plates.Count > maxHeight)
                maxHeight = gameManager.goalStacks[i].plates.Count;

        for (int row = maxHeight - 1; row >= 0; row--)
        {
            string line = "";
            for (int i = 0; i < gameManager.goalStacks.Length; i++)
            {
                List<Plate> plates = gameManager.goalStacks[i].plates;
                if (row < plates.Count)
                    // Colour tag + fixed padding after
                    line += colourSymbols[plates[row].id - 1] +
                        "<color=#00000000>xxx</color>";
                else
                    // Invisible placeholder same width as [X]xxx
                    line += "<color=#00000000>[X]xxx</color>";
            }
            display += line + "\n";
        }

        display += "------------------";
        goalText.text = display;
    }

    private void CheckWinUI()
    {
        for (int i = 0; i < 3; i++)
        {
            if (gameManager.stacks[i].plates.Count !=
                gameManager.goalStacks[i].plates.Count)
                return;

            for (int j = 0; j < gameManager.stacks[i].plates.Count; j++)
            {
                // Changed from colour/number to id
                if (gameManager.stacks[i].plates[j].id !=
                    gameManager.goalStacks[i].plates[j].id)
                    return;
            }
        }

        ShowWinScreen();
    }

    private void ShowWinScreen()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);

            if (winText != null)
                winText.text = "You solved it in " +
                    gameManager.moveCount + " moves!\n\n" +
                    GetPerformanceMessage();

            // Hide goal panel when won
            if (goalPanel != null)
                goalPanel.SetActive(false);
        }
    }

    private string GetPerformanceMessage()
    {
        int moves = gameManager.moveCount;
        if (moves <= 10)
            return "Outstanding!";
        else if (moves <= 20)
            return "Great job!";
        else if (moves <= 30)
            return "Well done!";
        else
            return "Keep practicing!";
    }

    private void RestartGame()
    {
        gameManager.RestartGame();
        RenderAllStacks();
        UpdateMoveCount();
        UpdateGoalDisplay();

        if (winPanel != null)
            winPanel.SetActive(false);

        if (goalPanel != null)
            goalPanel.SetActive(true);
    }
    private void ShowInvalidMoveFeedback(string message)
    {
        UIManager ui = FindObjectOfType<UIManager>();
        if (ui != null)
            ui.ShowFeedback(message);
    }

    private IEnumerator FlashMessage(string message)
    {
        if (moveCountText == null) yield break;

        string originalText = moveCountText.text;
        Color originalColor = moveCountText.color;

        moveCountText.text = message;
        moveCountText.color = Color.red;

        yield return new WaitForSeconds(1.2f);

        moveCountText.text = originalText;
        moveCountText.color = originalColor;
    }
}