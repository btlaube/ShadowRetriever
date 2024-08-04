using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuMusic : MonoBehaviour
{
    private AudioHandler audioHandler;
    public float fadeOutDuration;

    void Awake()
    {
        audioHandler = GetComponent<AudioHandler>();
    }

    void Start()
    {
        if (audioHandler != null)
            audioHandler.Play("Music");
    }
    
    // Function to start the fade-out process
    public void StartFadeOut()
    {
        if (audioHandler != null)
            StartCoroutine(FadeOut(audioHandler.GetAudioSource("Music"), fadeOutDuration));
    }

    // Coroutine to fade out the volume
    private IEnumerator FadeOut(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
            yield return null;
        }

        // Ensure the volume is set to 0 at the end of the fade-out
        audioSource.volume = 0;

        // Optionally stop the audio after fading out
        audioSource.Stop();
    }
}
