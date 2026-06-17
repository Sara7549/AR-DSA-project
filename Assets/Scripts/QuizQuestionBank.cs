// QuizQuestionBank.cs
// Create one asset per game via:
//   Right-click in Project  Create  Quiz  Question Bank
//
// Assign the three assets (Stack, Queue, LinkedList) to QuizManager
// in the Inspector. Questions are now edited in the Project window,
// not on scene objects.

using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class QuizQuestion
{
    [TextArea(2, 4)]
    public string question;
    public string[] options = new string[4];
    public int correctAnswerIndex;
    [TextArea(2, 5)]
    public string explanation;
}

[CreateAssetMenu(menuName = "Quiz/Question Bank", fileName = "QuizQuestionBank")]
public class QuizQuestionBank : ScriptableObject
{
    [Header("Basic — shown to players who struggled (many errors / slow)")]
    public List<QuizQuestion> basicQuestions = new List<QuizQuestion>();

    [Header("Medium — shown to players with moderate performance")]
    public List<QuizQuestion> mediumQuestions = new List<QuizQuestion>();

    [Header("Hard — shown to players who performed efficiently")]
    public List<QuizQuestion> hardQuestions = new List<QuizQuestion>();

    // <summary>Returns the correct list for a given level.</summary>
    public List<QuizQuestion> GetPool(PerformanceLevel level)
    {
        switch (level)
        {
            case PerformanceLevel.Hard: return hardQuestions;
            case PerformanceLevel.Medium: return mediumQuestions;
            default: return basicQuestions;
        }
    }
}