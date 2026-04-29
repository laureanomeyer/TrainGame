using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartRun()
    {
        SceneManager.LoadScene("Shop");
    }

    public void CloseGame()
    {
        Application.Quit();
    }
}
