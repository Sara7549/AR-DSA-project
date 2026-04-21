using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class QueueController : MonoBehaviour
{
    [Header("Lane Visuals")]
    public LaneVisual[] laneVisuals = new LaneVisual[3];

    [Header("Area Visuals")]
    public HoldingAreaVisual holdingVisual;
    public ExitZoneVisual exitVisual;

    [Header("UI")]
    public QueueUIManager uiManager;
    public Button restartButton;

    [Header("Drag")]
    public DragHandler dragHandler;

    private QueueGameManager gameManager;

    public void InitializeGame()
    {
        gameManager = QueueGameManager.Instance;
        gameManager.InitializeGame();

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }

        if (uiManager != null)
            uiManager.HideWinPanel();

        RenderAll();
        UpdateMoveCount();

        // Activate drag handler
        if (dragHandler != null)
            dragHandler.SetActive(true);
    }

    public void RenderAll()
    {
        // Render lanes
        for (int i = 0; i < laneVisuals.Length; i++)
        {
            if (laneVisuals[i] != null &&
                i < gameManager.lanes.Length)
                laneVisuals[i].RenderLane(
                    gameManager.lanes[i]);
        }

        // Render holding area
        if (holdingVisual != null)
            holdingVisual.RenderHolding(
                gameManager.holdingArea);
    }

    public void UpdateMoveCount()
    {
        if (uiManager != null)
            uiManager.UpdateMoveCount(gameManager.moveCount);
    }

    public bool TryMoveToHolding(int laneIndex)
    {
        bool success = gameManager.MoveToHolding(laneIndex);
        if (success)
        {
            RenderAll();
            UpdateMoveCount();
            CheckWin();
        }
        else
        {
            string reason = GetHoldingFailReason(laneIndex);
            if (uiManager != null)
                uiManager.ShowFeedback(reason);
        }
        return success;
    }

    private string GetHoldingFailReason(int laneIndex)
    {
        Vehicle front = gameManager.lanes[laneIndex].Front;
        if (front == null)
            return "Lane is empty!";
        if (front.isTarget)
            return "Cannot hold target car!";
        if (!gameManager.holdingArea.CanHold(front))
            return "Holding area is full or vehicle too big!";
        return "Cannot move there!";
    }

    public bool TryMoveFromHolding(Vehicle vehicle,
        int laneIndex)
    {
        bool success =
            gameManager.MoveFromHolding(vehicle, laneIndex);
        if (success)
        {
            RenderAll();
            UpdateMoveCount();
        }
        else
        {
            if (uiManager != null)
                uiManager.ShowFeedback("Lane is full!");
        }
        return success;
    }

    public bool TryMoveBetweenLanes(int fromLane, int toLane)
    {
        bool success =
            gameManager.MoveBetweenLanes(fromLane, toLane);
        if (success)
        {
            RenderAll();
            UpdateMoveCount();
        }
        else
        {
            if (uiManager != null)
                uiManager.ShowFeedback(
                    "Cannot move there — lane full!");
        }
        return success;
    }

    public bool TryExitTarget(int laneIndex)
    {
        bool success = gameManager.TryExitTarget(laneIndex);
        if (success)
        {
            RenderAll();
            UpdateMoveCount();
            CheckWin();
        }
        else
        {
            if (uiManager != null)
                uiManager.ShowFeedback(
                    "Target car is not at the front!");
        }
        return success;
    }

    public void CheckWin()
    {
        if (gameManager.IsGameWon())
        {
            if (uiManager != null)
                uiManager.ShowWinScreen(gameManager.moveCount);
        }
    }

    private void RestartGame()
    {
        if (exitVisual != null)
            exitVisual.Clear();

        gameManager.RestartGame();
        RenderAll();
        UpdateMoveCount();

        if (uiManager != null)
            uiManager.HideWinPanel();
    }
    public void GoToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public LaneVisual GetLaneVisual(int index)
    {
        if (index < 0 || index >= laneVisuals.Length) return null;
        return laneVisuals[index];
    }
    public bool TryLiftFromLane(int laneIndex)
    {
        return gameManager.TryLiftFromLane(laneIndex);
    }

    public void ReturnVehicleToLane(int laneIndex, Vehicle vehicle)
    {
        gameManager.ReturnToLane(laneIndex, vehicle);
    }
    public bool TryEnqueueToLane(Vehicle vehicle, int laneIndex)
    {
        return gameManager.TryEnqueueToLane(vehicle, laneIndex);
    }

    public bool TryAddToHolding(Vehicle vehicle)
    {
        bool success = gameManager.TryAddToHolding(vehicle);
        if (!success && uiManager != null)
            uiManager.ShowFeedback("Holding area is full!");
        return success;
    }

    public bool TryExitLifted(Vehicle vehicle)
    {
        return gameManager.TryExitLifted(vehicle);
    }
    // Rename existing private CheckWin to public
    public void CheckWinPublic()
    {
        if (gameManager.IsGameWon())
        {
            if (uiManager != null)
                uiManager.ShowWinScreen(gameManager.moveCount);
        }
    }
}