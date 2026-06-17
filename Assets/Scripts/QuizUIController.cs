// QuizUIController.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class QuizUIController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject quizPanel;
    public TextMeshProUGUI performanceFeedbackText;
    public TextMeshProUGUI questionNumberText;
    public TextMeshProUGUI questionText;
    public Button[] answerButtons;
    public TextMeshProUGUI[] answerTexts;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI explanationText;
    public Button nextButton;
    public GameObject resultsPanel;
    public TextMeshProUGUI resultsText;
    public Button returnToMenuButton;

    // Stores each button's original Inspector color so we can restore it
    private Color[] originalButtonColors;
    private bool hasAnswered = false;

    private void Start()
    {
        if (quizPanel != null) quizPanel.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);

        // Cache the original colors from the Inspector ONCE
        originalButtonColors = new Color[answerButtons.Length];
        for (int i = 0; i < answerButtons.Length; i++)
            if (answerButtons[i] != null)
                originalButtonColors[i] = answerButtons[i].image.color;

        // Wire up persistent buttons here so they are never missing
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextButtonClicked);
        }

        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.RemoveAllListeners();
            returnToMenuButton.onClick.AddListener(ReturnToMenu);
        }
    }

    // -------------------------------------------------------------------------
    // Entry point
    // -------------------------------------------------------------------------

    public void StartQuiz(string gameType)
    {
        PerformanceLevel level = ResolvePerformanceLevel(gameType);

        if (performanceFeedbackText != null)
            performanceFeedbackText.text =
                "Based on your performance you will answer "
                + level.ToString() + " level questions.";

        QuizManager.Instance.ResetQuiz();
        QuizManager.Instance.SelectQuestions(gameType, level, 3); // 3 questions

        if (quizPanel != null)
            quizPanel.SetActive(true);

        ShowCurrentQuestion();
    }

    // -------------------------------------------------------------------------
    // Question display
    // -------------------------------------------------------------------------

    private void ShowCurrentQuestion()
    {
        QuizQuestion question = QuizManager.Instance.GetCurrentQuestion();
        if (question == null)
        {
            Debug.LogWarning("[QuizUIController] No current question.");
            return;
        }

        hasAnswered = false;

        if (feedbackText != null) feedbackText.text = "";
        if (explanationText != null) explanationText.text = "";
        if (nextButton != null) nextButton.gameObject.SetActive(false);

        if (questionNumberText != null)
            questionNumberText.text =
                "Question " + (QuizManager.Instance.currentQuestionIndex + 1)
                + " / " + QuizManager.Instance.GetTotalQuestions();

        if (questionText != null)
            questionText.text = question.question;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] == null) continue;

            // Restore the original Inspector color instead of forcing white
            answerButtons[i].image.color = originalButtonColors[i];
            answerButtons[i].interactable = true;

            if (answerTexts != null && i < answerTexts.Length
                && answerTexts[i] != null && i < question.options.Length)
                answerTexts[i].text = question.options[i];

            int captured = i;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(
                () => OnAnswerSelected(captured));
        }
    }

    // -------------------------------------------------------------------------
    // Answer handling
    // -------------------------------------------------------------------------

    private void OnAnswerSelected(int index)
    {
        if (hasAnswered) return;
        hasAnswered = true;

        QuizQuestion question = QuizManager.Instance.GetCurrentQuestion();
        bool correct = QuizManager.Instance.AnswerQuestion(index);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] == null) continue;
            answerButtons[i].interactable = false;

            if (i == question.correctAnswerIndex)
                answerButtons[i].image.color = Color.green;
            else if (i == index && !correct)
                answerButtons[i].image.color = Color.red;
            // all other buttons keep their original color
        }

        if (feedbackText != null)
        {
            feedbackText.text = correct ? "Correct!" : "Incorrect";
            feedbackText.color = correct ? Color.green : Color.red;
        }

        if (explanationText != null)
            explanationText.text = question.explanation;

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true);
            TextMeshProUGUI label =
                nextButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = QuizManager.Instance.IsQuizComplete()
                    ? "See Results" : "Next Question";
        }
    }

    // -------------------------------------------------------------------------
    // Next button — wired in Start() so it is always connected
    // -------------------------------------------------------------------------

    public void OnNextButtonClicked()
    {
        if (QuizManager.Instance.IsQuizComplete())
            ShowResults();
        else
            ShowCurrentQuestion();
    }

    // -------------------------------------------------------------------------
    // Results
    // -------------------------------------------------------------------------

    private void ShowResults()
    {
        if (quizPanel != null) quizPanel.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(true);

        int score = QuizManager.Instance.GetScore();
        int total = QuizManager.Instance.GetTotalQuestions();
        float percentage = total > 0 ? (float)score / total * 100f : 0f;

        string performance =
            percentage >= 80 ? "Excellent understanding!" :
            percentage >= 60 ? "Good understanding!" :
            percentage >= 40 ? "Basic understanding." :
                               "Keep practicing!";

        if (resultsText != null)
            resultsText.text =
                "Quiz Complete!\n\n"
                + "Score: " + score + " / " + total + "\n"
                + "(" + Mathf.RoundToInt(percentage) + "%)\n\n"
                + performance;
    }

    // -------------------------------------------------------------------------
    // Navigation
    // -------------------------------------------------------------------------

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // -------------------------------------------------------------------------
    // Performance resolution
    // -------------------------------------------------------------------------

    private PerformanceLevel ResolvePerformanceLevel(string gameType)
    {
        switch (gameType)
        {
            case "Stack":
                {
                    StackStatisticsTracker t =
                        FindObjectOfType<StackStatisticsTracker>();
                    if (t != null) return t.DeterminePerformance();
                    break;
                }
            case "Queue":
                {
                    QueueStatisticsTracker t =
                        FindObjectOfType<QueueStatisticsTracker>();
                    if (t != null) return t.DeterminePerformance();
                    break;
                }
            case "LinkedList":
                {
                    LinkedListStatisticsTracker t =
                        FindObjectOfType<LinkedListStatisticsTracker>();
                    if (t != null) return t.DeterminePerformance();
                    break;
                }
        }

        Debug.LogWarning(
            $"[QuizUIController] No tracker found for '{gameType}'. Defaulting to Basic.");
        return PerformanceLevel.Basic;
    }
}