using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; 

public class MainMenuUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject firstSelectedButton; 

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        FocusFirstButton();
    }

    void OnEnable()
    {
        FocusFirstButton();
    }

    public void FocusFirstButton()
    {
        if (EventSystem.current != null && firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }
    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }


    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }
    public void Settings()
    {
        SceneManager.LoadScene(2);
    }
    public void Info()
    {
        SceneManager.LoadScene(3);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
