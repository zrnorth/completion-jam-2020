using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Game scalars
    // Hookups
    [SerializeField]
    private Player _player;
    [SerializeField]
    private Transform _playerSpawnPoint;

    private SceneTransitioner _transition;


    // Start is called before the first frame update
    void Start() {
        // Place the player
        _player.transform.position = _playerSpawnPoint.position;

        _transition = GetComponent<SceneTransitioner>();
    }

    private void Update() {
        // If player hits Esc, return to the main menu.
        //if (Input.GetKeyDown(KeyCode.Escape)) {
        //    SceneManager.LoadScene(0); // MainMenu
        //}
    }

    public void PlayerDied() {
        // TODO: play animation and pause
        _transition.LoadNextScene();
    }

    public void CompletedLevel() {
        // TODO: play a cool animation here
        StartCoroutine(CompletedLevelCoroutine());
    }

    IEnumerator CompletedLevelCoroutine() {
        _player.FreezePlayer();
        yield return new WaitForSeconds(3f);
        _transition.LoadNextScene();
    }
}
