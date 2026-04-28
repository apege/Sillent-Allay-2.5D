using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // tombol PLAY
    public void PlayGame()
    {
        SceneManager.LoadScene("Cutscene");
        // ganti "GameScene" sesuai nama scene lu
    }

    // tombol EXIT
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Closed"); // cuma keliatan di editor
    }
}
