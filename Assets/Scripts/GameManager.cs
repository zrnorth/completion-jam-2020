using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Game scalars
    [SerializeField]
    private float _deathDelay = 1f;
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

    private void ResetGameState() {
        Time.timeScale = 1f;
        _player.ResetControls();
        _player.ResetPhysics();
        if (_slowDownTimeCoroutine != null) {
            StopCoroutine(_slowDownTimeCoroutine);
        }
    }

    public void PlayerDied() {
        ResetGameState();
        StartCoroutine(LoadTransitionAfter(_deathDelay));
    }

    IEnumerator LoadTransitionAfter(float seconds) {
        yield return new WaitForSeconds(seconds);
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
