using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalScreenUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    private void Start()
    {
        RunResult result = GameManager.Instance.LastRunResult;

        if(result == RunResult.Victory)
        {
            titleText.text = "A job well done.";
            descriptionText.text = "Bronco Buckle, you've proved once again why folks call you a legend.";
        }
        else if (result == RunResult.Defeat)
        {
            titleText.text = "End of the line, Bronco Buckle";
            descriptionText.text = "Better luck next time...";
        }
        else
        {
            titleText.text = "Fin de la partida";
            descriptionText.text = "La partida termino";
        }
    }

    public void Restart()
    {
        GameManager.Instance.StartNewSession();
    }

    public void MainMenu()
    {
        GameManager.Instance.GoToMainMenu();
    }
}
