// LinkedListStatisticsTracker.cs
// Added hint tracking to performance evaluation.
// Hint levels: 1 = vague ("remove the red carriage")
//              2 = conceptual ("set prev.next = next")
//              3 = explicit ("drag this arrow to that carriage")
// A player who never used hints or only used level 1 shows better understanding.

using UnityEngine;

public class LinkedListStatisticsTracker : MonoBehaviour
{
    public static LinkedListStatisticsTracker Instance;

    public int totalConnections = 0;
    public int incorrectConnections = 0;
    public int undoCount = 0;
    public int tempPointerUsed = 0;
    public int garbageCollections = 0;

    // Hint tracking
    public int hintsLevel1 = 0;  // vague hints requested
    public int hintsLevel2 = 0;  // conceptual hints requested
    public int hintsLevel3 = 0;  // explicit hints requested

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
        totalConnections = 0;
        incorrectConnections = 0;
        undoCount = 0;
        tempPointerUsed = 0;
        garbageCollections = 0;
        hintsLevel1 = 0;
        hintsLevel2 = 0;
        hintsLevel3 = 0;
        timeElapsed = 0f;
    }

    public void RecordConnection() => totalConnections++;
    public void RecordIncorrectConnection() => incorrectConnections++;
    public void RecordUndo() => undoCount++;
    public void RecordTempPointerUse() => tempPointerUsed++;
    public void RecordGarbageCollection() => garbageCollections++;

    /// <summary>
    /// Call this from LinkedListGameManager.OnHintButtonPressed()
    /// passing the current hintLevel after it has been incremented.
    /// </summary>
    public void RecordHint(int level)
    {
        switch (level)
        {
            case 1: hintsLevel1++; break;
            case 2: hintsLevel2++; break;
            case 3: hintsLevel3++; break;
        }
    }

    public PerformanceLevel DeterminePerformance()
    {
        isTracking = false;

        float allAttempts = totalConnections + incorrectConnections;
        float errorRate = allAttempts > 0
            ? (float)incorrectConnections / allAttempts : 0f;

        bool usedUndoExcessively = undoCount > 3;
        bool causedGCEvents = garbageCollections > 1;
        float secPerMove = totalConnections > 0
            ? timeElapsed / totalConnections : 99f;

        // Hint score: level 3 hints are the most penalising (needed spoon-feeding),
        // level 2 moderate, level 1 minor
        int hintScore = hintsLevel3 * 3 + hintsLevel2 * 2 + hintsLevel1;

        // Hard:  clean connections, no excessive undo, no lost carriages,
        //        confident pace, and either no hints or only vague ones
        if (errorRate < 0.15f && !usedUndoExcessively
            && !causedGCEvents && secPerMove < 20f && hintScore <= 3)
            return PerformanceLevel.Hard;

        // Medium: some errors or undo, no GC disaster, didn't need explicit hints
        if (errorRate < 0.40f && garbageCollections <= 2
            && secPerMove < 40f && hintScore <= 7)
            return PerformanceLevel.Medium;

        // Basic: many wrong connections, lots of undo, lost carriages,
        //        or relied heavily on explicit hints
        return PerformanceLevel.Basic;
    }
}