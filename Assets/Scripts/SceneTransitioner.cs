using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitioner : MonoBehaviour
{
    [SerializeField]
    private int _sceneToLoad = -1;

    [SerializeField]
    private bool loadNextIndex = true;

    public void LoadNextScene()
    {
        Debug.Log("You win!");
        GameObject dontUnload = GameObject.Find("DontUnload");
        if (dontUnload)
        {
            foreach (Transform child in dontUnload.transform)
            {
                Destroy(child.gameObject);
            }
        }
        if (loadNextIndex)
        {
            int buildIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(buildIndex + 1);
        }
        else
        {
            SceneManager.LoadScene(_sceneToLoad);
        }
    }
}
