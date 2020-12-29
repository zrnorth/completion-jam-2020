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
    private Coroutine _slowDownTimeCoroutine;


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

    private void ResetGameState() {
        Time.timeScale = 1f;
        _player.ResetControls();
        if (_slowDownTimeCoroutine != null) {
            StopCoroutine(_slowDownTimeCoroutine);
        }
    }

    public void PlayerDied() {
        // TODO: play animation and pause
        ResetGameState();
        _transition.LoadScene();
    }

    public void CompletedLevel() {
        // TODO: play a cool animation here
        ResetGameState();
        StartCoroutine(CompletedLevelCoroutine());
    }

    IEnumerator CompletedLevelCoroutine() {
        _player.FreezePlayer();
        yield return new WaitForSeconds(3f);
        _transition.LoadScene();
    }

    public void SlowDownTime() {
        _slowDownTimeCoroutine = StartCoroutine(RelayEffectSlowDownTime());
    }

    // This effect is caused by the SlowDownTime relay. It gradually slows the game down until 0, then player dies.
    public IEnumerator RelayEffectSlowDownTime() {
        Time.timeScale = 0.8f;
        yield return new WaitForSecondsRealtime(2.5f);
        Time.timeScale = 0.6f;
        yield return new WaitForSecondsRealtime(2f);
        Time.timeScale = 0.4f;
        yield return new WaitForSecondsRealtime(2f);
        Time.timeScale = 0.2f;
        yield return new WaitForSecondsRealtime(2f);
        Time.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(1.5f);
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(1.5f);
        Time.timeScale = 1.0f;
        PlayerDied();
    }
}
