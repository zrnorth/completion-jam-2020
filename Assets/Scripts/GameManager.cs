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


    // Start is called before the first frame update
    void Start() {
        // Place the player
        _player.transform.position = _playerSpawnPoint.position;
    }

    private void Update() {
        // If player hits Esc, return to the main menu.
        if (Input.GetKeyDown(KeyCode.Escape)) {
            SceneManager.LoadScene(0); // MainMenu
        }
    }

    public void PlayerDied() {
        _player.transform.position = _playerSpawnPoint.position;

        // If we disable and reenable the player, we reset all physics and velocity values.
        _player.gameObject.SetActive(false);
        _player.gameObject.SetActive(true);
    }

    public void CompletedLevel(int nextLevel) {
        StartCoroutine(CompletedLevelCoroutine(nextLevel));
    }

    IEnumerator CompletedLevelCoroutine(int nextLevel) {
        _player.FreezePlayer();
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(nextLevel);
    }
}
