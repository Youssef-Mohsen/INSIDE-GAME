using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public void OnResumeClicked()
    {
        Pause_Manager.Instance.ResumeGame();
    }
}