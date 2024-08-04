using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader instance;
    public Animator transition;
    // public CanvasGroupScript canvasGroup;

    [SerializeField] private float transitionTime = 1f;

    void Awake() {
        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(int sceneToLoad) {
        StartCoroutine(LoadLevel(sceneToLoad));
    }

    IEnumerator LoadLevel(int levelIndex) {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(levelIndex);

        transition.SetTrigger("End");
    }

    public void Quit() {
        Application.Quit();
    }
}
