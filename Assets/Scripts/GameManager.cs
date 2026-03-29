using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameStack[] stacks = new GameStack[3];
    public GameStack[] goalStacks = new GameStack[3];

    private int selectedStackIndex = -1;
    public int moveCount = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void InitializeGame()
    {
        // Create stacks
        for (int i = 0; i < 3; i++)
        {
            stacks[i] = new GameStack();
            goalStacks[i] = new GameStack();
        }

        // Distribute plates evenly across stacks 0 and 1
        // Stack 0: Red1, Red2, Red3 (bottom to top)
        // Stack 1: Blue1, Blue2, Blue3 (bottom to top)
        // Stack 2: empty
        stacks[0].Push(new Plate(PlateColour.Red, 1));
        stacks[0].Push(new Plate(PlateColour.Red, 2));
        stacks[0].Push(new Plate(PlateColour.Red, 3));

        stacks[1].Push(new Plate(PlateColour.Blue, 1));
        stacks[1].Push(new Plate(PlateColour.Blue, 2));
        stacks[1].Push(new Plate(PlateColour.Blue, 3));

        // Shuffle to create GOAL (20 moves)
        ShuffleStacks(stacks, 20);

        // Copy shuffled state to goalStacks
        for (int i = 0; i < 3; i++)
        {
            goalStacks[i] = new GameStack();
            foreach (Plate p in stacks[i].plates)
                goalStacks[i].Push(new Plate(p.colour, p.number));
        }

        // Shuffle more to create START (20 more moves)
        ShuffleStacks(stacks, 20);

        moveCount = 0;
        selectedStackIndex = -1;

        
    }

    private void ShuffleStacks(GameStack[] targetStacks,
        int moves)
    {
        System.Random rng = new System.Random();

        for (int i = 0; i < moves; i++)
        {
            List<Vector2Int> validMoves = new List<Vector2Int>();

            for (int from = 0; from < 3; from++)
            {
                if (targetStacks[from].IsEmpty()) continue;

                for (int to = 0; to < 3; to++)
                {
                    if (from == to) continue;
                    if (!targetStacks[to].IsFull())
                        validMoves.Add(new Vector2Int(from, to));
                }
            }

            if (validMoves.Count == 0) break;

            Vector2Int move =
                validMoves[rng.Next(validMoves.Count)];
            Plate plate = targetStacks[move.x].Pop();
            targetStacks[move.y].Push(plate);
        }
    }

    public void OnStackTapped(int stackIndex)
    {
        if (selectedStackIndex == -1)
        {
            if (stacks[stackIndex].IsEmpty())
            {
                Debug.Log("Cannot select empty stack");
                return;
            }
            selectedStackIndex = stackIndex;
            return;
        }

        if (selectedStackIndex == stackIndex)
        {
            selectedStackIndex = -1;
            return;
        }

        TryMove(selectedStackIndex, stackIndex);
        selectedStackIndex = -1;
    }

    private void TryMove(int fromIndex, int toIndex)
    {
        if (stacks[toIndex].IsFull())
        {
            Debug.Log("Stack " + toIndex + " is full");
            return;
        }

        Plate plate = stacks[fromIndex].Pop();
        stacks[toIndex].Push(plate);
        moveCount++;

        PrintStacks(stacks);
        CheckWin();
    }

    public void CheckWin()
    {
        for (int i = 0; i < 3; i++)
        {
            if (stacks[i].plates.Count !=
                goalStacks[i].plates.Count)
                return;

            for (int j = 0; j < stacks[i].plates.Count; j++)
            {
                if (stacks[i].plates[j].colour !=
                    goalStacks[i].plates[j].colour ||
                    stacks[i].plates[j].number !=
                    goalStacks[i].plates[j].number)
                    return;
            }
        }

        Debug.Log("YOU WIN in " + moveCount + " moves!");
    }

    public int GetSelectedStack()
    {
        return selectedStackIndex;
    }

    public void RestartGame()
    {
        moveCount = 0;
        selectedStackIndex = -1;
        InitializeGame();
    }

    private void PrintStacks(GameStack[] targetStacks)
    {
        for (int i = 0; i < targetStacks.Length; i++)
        {
            string contents = "Stack " + i + ": ";
            foreach (Plate p in targetStacks[i].plates)
                contents += "[" + p + "] ";
        }
    }
}