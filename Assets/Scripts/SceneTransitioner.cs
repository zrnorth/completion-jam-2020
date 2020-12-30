using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitioner : MonoBehaviour
{
    enum Type
    {
        ReloadScene,
        LoadNextScene,
        LoadSpecifiedScene
    }
    [SerializeField]
    private Type type;
    [SerializeField]
    private int _specifiedSceneIfNeeded = -1;
    [SerializeField]
    private AudioClip _transitionClip;

    public void LoadScene() {
        Time.timeScale = 1f;
        int currentSceneIdx = SceneManager.GetActiveScene().buildIndex;
        switch (type) {
            case Type.ReloadScene:
                StartCoroutine(LoadSceneAfterClipFinishes(currentSceneIdx));
                return;
            case Type.LoadNextScene:
                StartCoroutine(LoadSceneAfterClipFinishes(currentSceneIdx + 1));
                return;
            case Type.LoadSpecifiedScene:
                StartCoroutine(LoadSceneAfterClipFinishes(_specifiedSceneIfNeeded));
                return;
            default:
                return;
        }
    }

    private IEnumerator LoadSceneAfterClipFinishes(int scene) {
        if (_transitionClip != null) {
            AudioSource.PlayClipAtPoint(_transitionClip, transform.position);
            yield return new WaitForSecondsRealtime(0.75f); // Hack here
        }
        // If there's a game manager, tell it to reset state before loading next scene,
        // in case it's the exact same scene.
        GameObject gameManager = GameObject.Find("Game Manager");
        if (gameManager != null) {
            gameManager.GetComponent<GameManager>().ResetGameState();
        }

        SceneManager.LoadScene(scene);
    }
}
