// IPerformanceTracker.cs
// Place anywhere in your Assets/Scripts folder.
// Every game-specific tracker implements this so QuizUIController
// never has to know which scene it is running in.

public interface IPerformanceTracker
{
    /// <summary>
    /// Evaluates all recorded metrics and returns a difficulty level
    /// for the follow-up quiz. Also stops the timer.
    /// </summary>
    PerformanceLevel DeterminePerformance();
}

/// <summary>
/// Shared enum used by all trackers and the quiz system.
/// Defined here so it is not nested inside StackStatisticsTracker.
/// Update every existing reference from
///   StackStatisticsTracker.PerformanceLevel
/// to just
///   PerformanceLevel
/// </summary>
public enum PerformanceLevel { Basic, Medium, Hard }