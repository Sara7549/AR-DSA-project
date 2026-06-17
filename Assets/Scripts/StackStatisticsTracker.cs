// StackStatisticsTracker.cs
// Regret moves removed — they fire on valid intermediate moves in stack puzzles
// (you often must move a bowl off its correct stack to unblock one beneath it).
//
// Performance is now judged on three clean axes:
//   1. totalMoves  — synced from gameManager.moveCount at win time (always accurate)
//   2. secPerMove  — how long the player thought between moves
//   3. invalidMoves — tried to place on a full stack
//
// Thresholds are based on real test data:
//   Good run: moves=6,  secPerMove=2.1s → Hard
//   Bad run:  moves=54, secPerMove=~5s  → Basic

using UnityEngine;

public class StackStatisticsTracker : MonoBehaviour
{
    public static StackStatisticsTracker Instance;

    public int totalMoves = 0;   // set by SyncFromGameManager(), not incremented
    public int invalidMoves = 0;
    public float timeElapsed = 0f;
    public bool isTracking = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (isTracking) timeElapsed += Time.deltaTime;
    }

    public void StartTracking()
    {
        isTracking = true;
        totalMoves = 0;
        invalidMoves = 0;
        timeElapsed = 0f;
    }

    // Does nothing — move count comes from SyncFromGameManager()
    public void RecordMove() { }

    public void RecordInvalidMove() => invalidMoves++;

    // Call this at the start of ShowWinScreen(), before StartQuiz()
    public void SyncFromGameManager(int realMoveCount)
    {
        totalMoves = realMoveCount;
    }

    public PerformanceLevel DeterminePerformance()
    {
        isTracking = false;

        float secPerMove = totalMoves > 0
            ? timeElapsed / totalMoves : 99f;

        float allAttempts = totalMoves + invalidMoves;
        float errorRate = allAttempts > 0
            ? (float)invalidMoves / allAttempts : 0f;

        // Hard:   solved efficiently and quickly, few invalid attempts
        //         good run was moves=6, secPerMove=2.1 → passes comfortably
        if (totalMoves <= 15 && secPerMove < 8f && errorRate < 0.15f)
            return PerformanceLevel.Hard;

        // Medium: solved but took more moves or more time
        //         catches someone who got there eventually but struggled
        if (totalMoves <= 35 && secPerMove < 20f && errorRate < 0.30f)
            return PerformanceLevel.Medium;

        // Basic:  bad run was moves=54, time=300s → falls here
        return PerformanceLevel.Basic;
    }
}