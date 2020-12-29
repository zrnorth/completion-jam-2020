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

    public void LoadScene() {
        Time.timeScale = 1f;
        int currentSceneIdx = SceneManager.GetActiveScene().buildIndex;

        switch (type) {
            case Type.ReloadScene:
                SceneManager.LoadScene(currentSceneIdx);
                return;
            case Type.LoadNextScene:
                SceneManager.LoadScene(currentSceneIdx + 1);
                return;
            case Type.LoadSpecifiedScene:
                SceneManager.LoadScene(_specifiedSceneIfNeeded);
                return;
            default:
                return;

        }
    }
}
