using UnityEngine;

public class MainMenuPanels : MonoBehaviour
{
    public GameObject howToPlayPanel;
    public GameObject aboutPanel;

    public void ShowHowToPlay()
    {
        howToPlayPanel.SetActive(true);
        aboutPanel.SetActive(false);
    }

    public void ShowAbout()
    {
        aboutPanel.SetActive(true);
        howToPlayPanel.SetActive(false);
    }

    public void HideAll()
    {
        howToPlayPanel.SetActive(false);
        aboutPanel.SetActive(false);
    }
}