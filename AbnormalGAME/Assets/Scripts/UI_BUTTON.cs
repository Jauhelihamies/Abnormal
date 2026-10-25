using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_BUTTON : MonoBehaviour
{
    public int LevelNumber = 0;
    public void OpenLevel()
    {
        SceneManager.LoadScene(LevelNumber);
    }
   
}
