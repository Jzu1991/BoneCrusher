using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MainMenuManager - Controla el menú principal de Bone Crusher.
/// Adjuntar al Canvas del menú. Los botones llaman a StartGame() y QuitGame().
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "DemoFloors30to25";

    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
