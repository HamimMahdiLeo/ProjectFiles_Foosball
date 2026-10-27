using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels & Buttons")]
    public GameObject instructionsPanel;
    public GameObject selectSidePanel; 
    public GameObject[] menuButtons; // Drag your 4 individual buttons here directly

    [Header("Links")]
    public string googleFormUrl = "https://docs.google.com/forms/d/1n10Xxc1BAjbht95QYK520ul-ahuhA9GxpUiwkB2N4bs/";

    void Update()
    {
        if (instructionsPanel != null && instructionsPanel.activeSelf && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseInstruction();
        }
        else if (selectSidePanel != null && selectSidePanel.activeSelf && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseSelectSide();
        }
    }

    public void PlayGame()
    {
        if (selectSidePanel != null) selectSidePanel.SetActive(true);
        ToggleMenuButtons(false); // Hides the main buttons safely
    }

    public void CloseSelectSide()
    {
        if (selectSidePanel != null) selectSidePanel.SetActive(false);
        ToggleMenuButtons(true); // Shows the main buttons again
    }

    void ToggleMenuButtons(bool state)
    {
        foreach (var btn in menuButtons)
        {
            if (btn != null) btn.SetActive(state);
        }
    }

    public void OpenInstruction()
    {
        if (instructionsPanel != null) instructionsPanel.SetActive(true);
    }

    public void CloseInstruction()
    {
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
    }

    public void OpenFeedback()
    {
        Application.OpenURL(googleFormUrl);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}