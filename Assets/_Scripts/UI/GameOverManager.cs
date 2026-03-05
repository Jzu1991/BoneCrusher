using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameOverManager - Controla los paneles de Game Over y Victoria.
/// Los botones llaman a Restart() y Quit().
/// </summary>
public class GameOverManager : MonoBehaviour
{
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        GameManager.QuitApplication();
    }
}
