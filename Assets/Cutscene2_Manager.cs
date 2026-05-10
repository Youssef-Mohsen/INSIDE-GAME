using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Cutscene2_Manager : MonoBehaviour
{
    [SerializeField] float cutsceneDuration = 5f; // change seconds as you want

    void Start()
    {
        StartCoroutine(WaitAndFinish());
    }

    IEnumerator WaitAndFinish()
    {
        yield return new WaitForSecondsRealtime(cutsceneDuration);
        CloseCutscene();
    }

    void CloseCutscene()
    {
        Time.timeScale = 1f;
        SceneManager.UnloadSceneAsync("cutScene 2");
    }
}