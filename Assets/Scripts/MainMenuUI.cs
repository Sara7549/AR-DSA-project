using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
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
    public void QuitApp()
    {
        Application.Quit();
    }
}