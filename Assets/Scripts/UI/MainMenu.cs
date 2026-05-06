using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Texture2D cursor;
    private void Awake()
    {
        Cursor.SetCursor(cursor, new Vector2(256, 256), CursorMode.Auto);
    }

    public void CloseGame()
    {
        Application.Quit();
    }
}
