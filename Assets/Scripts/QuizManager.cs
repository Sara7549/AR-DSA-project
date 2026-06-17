//using UnityEngine;
//using System.Collections.Generic;

//[System.Serializable]
//public class QuizQuestion
//{
//    public string question;
//    public string[] options = new string[4];
//    public int correctAnswerIndex;
//    public string explanation;
//}

//public class QuizManager : MonoBehaviour
//{
//    public static QuizManager Instance;

//    [Header("Stack Questions")]
//    public List<QuizQuestion> stackBasicQuestions;
//    public List<QuizQuestion> stackMediumQuestions;
//    public List<QuizQuestion> stackHardQuestions;

//    [Header("Queue Questions")]
//    public List<QuizQuestion> queueBasicQuestions;
//    public List<QuizQuestion> queueMediumQuestions;
//    public List<QuizQuestion> queueHardQuestions;

//    [Header("Linked List Questions")]
//    public List<QuizQuestion> linkedListBasicQuestions;
//    public List<QuizQuestion> linkedListMediumQuestions;
//    public List<QuizQuestion> linkedListHardQuestions;

//    public List<QuizQuestion> selectedQuestions;
//    public int currentQuestionIndex = 0;
//    private int correctAnswers = 0;

//    private void Awake()
//    {
//        Instance = this;
//    }

//    public List<QuizQuestion> SelectQuestions(
//        string gameType,
//        StackStatisticsTracker.PerformanceLevel level,
//        int numberOfQuestions = 5)
//    {
//        List<QuizQuestion> pool = new List<QuizQuestion>();

//        // Select question pool based on game and performance
//        if (gameType == "Stack")
//        {
//            switch (level)
//            {
//                case StackStatisticsTracker.PerformanceLevel.Basic:
//                    pool = stackBasicQuestions;
//                    break;
//                case StackStatisticsTracker.PerformanceLevel.Medium:
//                    pool = stackMediumQuestions;
//                    break;
//                case StackStatisticsTracker.PerformanceLevel.Hard:
//                    pool = stackHardQuestions;
//                    break;
//            }
//        }
//        else if (gameType == "Queue")
//        {
//            switch (level)
//            {
//                case StackStatisticsTracker.PerformanceLevel.Basic:
//                    pool = queueBasicQuestions;
//                    break;
//                case StackStatisticsTracker.PerformanceLevel.Medium:
//                    pool = queueMediumQuestions;
//                    break;
//                case StackStatisticsTracker.PerformanceLevel.Hard:
//                    pool = queueHardQuestions;
//                    break;
//            }
//        }
//        else if (gameType == "LinkedList")
//        {
//            switch (level)
//            {
//                case StackStatisticsTracker.PerformanceLevel.Basic:
//                    pool = linkedListBasicQuestions;
//                    break;
//                case StackStatisticsTracker.PerformanceLevel.Medium:
//                    pool = linkedListMediumQuestions;
//                    break;
//                case StackStatisticsTracker.PerformanceLevel.Hard:
//                    pool = linkedListHardQuestions;
//                    break;
//            }
//        }

//        // Randomly select questions from pool
//        List<QuizQuestion> selected = new List<QuizQuestion>();
//        List<QuizQuestion> shuffled = new List<QuizQuestion>(pool);

//        // Shuffle
//        for (int i = shuffled.Count - 1; i > 0; i--)
//        {
//            int rand = Random.Range(0, i + 1);
//            QuizQuestion temp = shuffled[i];
//            shuffled[i] = shuffled[rand];
//            shuffled[rand] = temp;
//        }

//        // Take first n questions
//        for (int i = 0; i < Mathf.Min(numberOfQuestions,
//            shuffled.Count); i++)
//            selected.Add(shuffled[i]);

//        selectedQuestions = selected;
//        return selected;
//    }

//    public QuizQuestion GetCurrentQuestion()
//    {
//        if (currentQuestionIndex < selectedQuestions.Count)
//            return selectedQuestions[currentQuestionIndex];
//        return null;
//    }

//    public bool AnswerQuestion(int answerIndex)
//    {
//        QuizQuestion current = GetCurrentQuestion();
//        if (current == null) return false;

//        bool correct = answerIndex == current.correctAnswerIndex;
//        if (correct) correctAnswers++;

//        currentQuestionIndex++;
//        return correct;
//    }

//    public bool IsQuizComplete()
//    {
//        return currentQuestionIndex >= selectedQuestions.Count;
//    }

//    public int GetScore()
//    {
//        return correctAnswers;
//    }

//    public int GetTotalQuestions()
//    {
//        return selectedQuestions.Count;
//    }

//    public void ResetQuiz()
//    {
//        currentQuestionIndex = 0;
//        correctAnswers = 0;
//    }
//}

// QuizManager.cs  — drop-in replacement for your existing QuizManager.cs
// Changes:
//   • Uses QuizQuestionBank ScriptableObjects instead of Inspector lists
//   • Works with the shared PerformanceLevel enum (not the nested one)
//   • Assigns correct bank automatically based on gameType string

using UnityEngine;
using System.Collections.Generic;

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance;

    [Header("Question Banks (assign .asset files in Inspector)")]
    public QuizQuestionBank stackBank;
    public QuizQuestionBank queueBank;
    public QuizQuestionBank linkedListBank;

    // Populated by SelectQuestions(); read by QuizUIController
    [HideInInspector]
    public List<QuizQuestion> selectedQuestions
        = new List<QuizQuestion>();
    [HideInInspector] public int currentQuestionIndex = 0;

    private int correctAnswers = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Picks <paramref name="count"/> random questions from the bank that
    /// matches <paramref name="gameType"/> and <paramref name="level"/>.
    /// </summary>
    public List<QuizQuestion> SelectQuestions(
        string gameType,
        PerformanceLevel level,
        int count = 5)
    {
        QuizQuestionBank bank = BankFor(gameType);
        if (bank == null)
        {
            Debug.LogError(
                $"[QuizManager] No bank found for game type '{gameType}'. "
                + "Check that stackBank / queueBank / linkedListBank are assigned.");
            selectedQuestions = new List<QuizQuestion>();
            return selectedQuestions;
        }

        List<QuizQuestion> pool = bank.GetPool(level);
        selectedQuestions = Shuffle(pool, count);
        return selectedQuestions;
    }

    public QuizQuestion GetCurrentQuestion()
    {
        if (currentQuestionIndex < selectedQuestions.Count)
            return selectedQuestions[currentQuestionIndex];
        return null;
    }

    /// <summary>Records the answer, advances the index, returns whether correct.</summary>
    public bool AnswerQuestion(int answerIndex)
    {
        QuizQuestion q = GetCurrentQuestion();
        if (q == null) return false;

        bool correct = answerIndex == q.correctAnswerIndex;
        if (correct) correctAnswers++;
        currentQuestionIndex++;
        return correct;
    }

    public bool IsQuizComplete()
        => currentQuestionIndex >= selectedQuestions.Count;

    public int GetScore() => correctAnswers;
    public int GetTotalQuestions() => selectedQuestions.Count;

    public void ResetQuiz()
    {
        currentQuestionIndex = 0;
        correctAnswers = 0;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private QuizQuestionBank BankFor(string gameType)
    {
        switch (gameType)
        {
            case "Stack": return stackBank;
            case "Queue": return queueBank;
            case "LinkedList": return linkedListBank;
            default:
                Debug.LogWarning(
                    $"[QuizManager] Unknown game type '{gameType}'.");
                return null;
        }
    }

    private static List<QuizQuestion> Shuffle(
        List<QuizQuestion> source, int take)
    {
        List<QuizQuestion> copy = new List<QuizQuestion>(source);

        // Fisher-Yates shuffle
        for (int i = copy.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (copy[i], copy[j]) = (copy[j], copy[i]);
        }

        List<QuizQuestion> result = new List<QuizQuestion>();
        for (int i = 0; i < Mathf.Min(take, copy.Count); i++)
            result.Add(copy[i]);

        return result;
    }
}