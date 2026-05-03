using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public GameObject howToPlayPanel;
    public GameObject aboutPanel;

    private void Start()
    {
        // Make sure panels are hidden at start
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
        if (aboutPanel != null)
            aboutPanel.SetActive(false);
    }

    public void StartStack()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void StartQueue()
    {
        SceneManager.LoadScene("QueueScene");
    }

    public void StartList()
    {
        SceneManager.LoadScene("ListScene");
    }

    public void ShowHowToPlay()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(true);
        if (aboutPanel != null)
            aboutPanel.SetActive(false);
    }

    public void ShowAbout()
    {
        if (aboutPanel != null)
            aboutPanel.SetActive(true);
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
    }

    public void HideAllPanels()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
        if (aboutPanel != null)
            aboutPanel.SetActive(false);
    }

    public void QuitApp()
    {
        Application.Quit();
    }
}