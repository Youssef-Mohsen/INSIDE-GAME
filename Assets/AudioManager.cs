using UnityEngine;

public class AudioToggle : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioSource sfxSource;

    private bool musicOn = true;
    private bool sfxOn = true;

    public void ToggleMusic()
    {
        musicOn = !musicOn;
        musicSource.mute = !musicOn;
    }

    public void ToggleSFX()
    {
        sfxOn = !sfxOn;
        sfxSource.mute = !sfxOn;
    }
}