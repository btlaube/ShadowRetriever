using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InSceneLevelLoader : MonoBehaviour
{
    private LevelLoader levelLoader;

    void Start()
    {
        levelLoader = LevelLoader.instance;
    }

    public void LoadScene(int scene)
    {
        levelLoader.LoadScene(scene);
    }
}
