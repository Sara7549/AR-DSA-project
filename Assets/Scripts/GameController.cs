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
            goalPanel.SetActive(true);

        RenderAllStacks();
        UpdateMoveCount();
        UpdateGoalDisplay();
    }

    public void OnStackTapped(int stackIndex)
    {
        if (isAnimating) return;

        int previousSelected = gameManager.GetSelectedStack();
        gameManager.OnStackTapped(stackIndex);
        int newSelected = gameManager.GetSelectedStack();

        for (int i = 0; i < stackVisuals.Length; i++)
            stackVisuals[i].SetSelected(i == newSelected);

        if (previousSelected != -1 &&
            newSelected == -1 &&
            previousSelected != stackIndex)
        {
            StartCoroutine(AnimateAndRender(
                previousSelected, stackIndex));
        }

        UpdateMoveCount();
    }

    private IEnumerator AnimateAndRender(int fromIndex,
        int toIndex)
    {
        isAnimating = true;

        yield return StartCoroutine(
            stackVisuals[fromIndex].AnimateMoveTo(
                stackVisuals[toIndex]));

        stackVisuals[fromIndex].RenderStack(
            gameManager.stacks[fromIndex]);
        stackVisuals[toIndex].RenderStack(
            gameManager.stacks[toIndex]);

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

        string display = "TARGET:\n";
        display += "S1      S2      S3\n";
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
                List<Plate> plates =
                    gameManager.goalStacks[i].plates;
                if (row < plates.Count)
                {
                    string colour = plates[row].colour ==
                        PlateColour.Red ? "R" : "B";
                    line += "[" + colour + plates[row].number +
                        "]  ";
                }
                else
                {
                    line += "      ";
                }
            }
            display += line + "\n";
        }

        display += "-----------------";
        goalText.text = display;
    }

    private void CheckWinUI()
    {
        for (int i = 0; i < 3; i++)
        {
            if (gameManager.stacks[i].plates.Count !=
                gameManager.goalStacks[i].plates.Count)
                return;

            for (int j = 0; j < gameManager.stacks[i]
                .plates.Count; j++)
            {
                if (gameManager.stacks[i].plates[j].colour !=
                    gameManager.goalStacks[i].plates[j].colour ||
                    gameManager.stacks[i].plates[j].number !=
                    gameManager.goalStacks[i].plates[j].number)
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
}