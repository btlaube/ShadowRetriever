using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneAudioManager : MonoBehaviour
{
    private AudioHandler audioHandler;
    private bool startedMusic;

    void Awake()
    {
        audioHandler = GetComponent<AudioHandler>();
    }

    public void StartGameMusic()
    {
        if (audioHandler != null)
            audioHandler.Play("Music");
    }
}
