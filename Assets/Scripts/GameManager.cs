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
        for (int i = 0; i < 3; i++)
        {
            stacks[i] = new GameStack();
            goalStacks[i] = new GameStack();
        }

        // Create 6 unique bowls
        stacks[0].Push(new Plate(1));
        stacks[0].Push(new Plate(2));
        stacks[0].Push(new Plate(3));
        stacks[1].Push(new Plate(4));
        stacks[1].Push(new Plate(5));
        stacks[1].Push(new Plate(6));

        // Shuffle to create GOAL
        ShuffleStacks(stacks, 20);

        // Copy to goalStacks
        for (int i = 0; i < 3; i++)
        {
            goalStacks[i] = new GameStack();
            foreach (Plate p in stacks[i].plates)
                goalStacks[i].Push(new Plate(p.id));
        }

        // Shuffle more to create START
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
            if (stacks[stackIndex].IsEmpty()) return;
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
        if (stacks[toIndex].IsFull()) return;

        int distanceBefore = ComputeGoalDistance();

        Plate plate = stacks[fromIndex].Pop();
        stacks[toIndex].Push(plate);
        moveCount++;

        int distanceAfter = ComputeGoalDistance();

        CheckWin();
    }

    public int ComputeGoalDistance()
    {
        int mismatches = 0;

        for (int i = 0; i < 3; i++)
        {
            List<Plate> current = stacks[i].plates;
            List<Plate> goal = goalStacks[i].plates;

            int maxHeight = Mathf.Max(current.Count, goal.Count);

            for (int j = 0; j < maxHeight; j++)
            {
                // A slot that exists in one but not the other is a mismatch
                if (j >= current.Count || j >= goal.Count)
                {
                    mismatches++;
                    continue;
                }

                if (current[j].id != goal[j].id)
                    mismatches++;
            }
        }

        return mismatches;
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
                if (stacks[i].plates[j].id !=
                    goalStacks[i].plates[j].id)
                    return;
            }
        }
    }

    public int GetSelectedStack() => selectedStackIndex;

    public void RestartGame()
    {
        moveCount = 0;
        selectedStackIndex = -1;
        InitializeGame();
    }
}