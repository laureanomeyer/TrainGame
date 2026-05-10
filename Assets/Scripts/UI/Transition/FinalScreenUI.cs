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
            titleText.text = "Victoria";
            descriptionText.text = "Llegaste a la ultima estacion";
        }
        else if (result == RunResult.Defeat)
        {
            titleText.text = "Derrota";
            descriptionText.text = "La locomotora fue destruida";
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
