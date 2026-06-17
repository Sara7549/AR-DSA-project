// QueueStatisticsTracker.cs
using UnityEngine;

public class QueueStatisticsTracker : MonoBehaviour
{
    public static QueueStatisticsTracker Instance;

    public int totalMoves = 0;
    public int invalidMoves = 0;
    public int holdingAreaViolations = 0;
    public int frontAccessViolations = 0;
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
        holdingAreaViolations = 0;
        frontAccessViolations = 0;
        timeElapsed = 0f;
    }

    public void RecordMove() => totalMoves++;
    public void RecordInvalidMove() => invalidMoves++;
    public void RecordHoldingViolation() => holdingAreaViolations++;
    public void RecordFrontAccessViolation() => frontAccessViolations++;

    public PerformanceLevel DeterminePerformance()
    {
        isTracking = false;

        float allAttempts = totalMoves + invalidMoves;
        float errorRate = allAttempts > 0
            ? (float)invalidMoves / allAttempts : 0f;

        bool understoodFIFO = frontAccessViolations <= 2;
        bool understoodHolding = holdingAreaViolations <= 2;

        float secPerMove = totalMoves > 0
            ? timeElapsed / totalMoves : 99f;

        if (errorRate < 0.30f && understoodFIFO && understoodHolding && secPerMove < 25f)
            return PerformanceLevel.Hard;

        if (errorRate < 0.45f && understoodFIFO && secPerMove < 45f)
            return PerformanceLevel.Medium;

        return PerformanceLevel.Basic;
    }
}