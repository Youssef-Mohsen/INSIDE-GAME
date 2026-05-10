using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause_Manager : MonoBehaviour
{
    public static Pause_Manager Instance; // ADD THIS
    private bool isPaused = false;

    void Awake()
    {
        Instance = this; // ADD THIS
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
                OpenMenu();
            else
                ResumeGame();
        }
    }

    public void OpenMenu()
    {
        SceneManager.LoadSceneAsync("pauseMenu", LoadSceneMode.Additive);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        SceneManager.UnloadSceneAsync("pauseMenu");
        Time.timeScale = 1f;
        isPaused = false;
    }
}