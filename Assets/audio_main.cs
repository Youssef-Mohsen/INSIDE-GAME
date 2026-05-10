using UnityEngine;
using System.Collections;
public class audio_main : MonoBehaviour
{
    public AudioSource backgroundMusic;
    public AudioSource oneShotAudio;

    void Start()
    {
        backgroundMusic.Play();   // loops forever
    }

    public void PlaySpecialSound()
    {

        StartCoroutine(PlayAfterDelay());
    }
    IEnumerator PlayAfterDelay()
    {
        yield return new WaitForSeconds(3.0f);
        oneShotAudio.Play();
    }
}