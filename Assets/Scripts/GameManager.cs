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

    public void ResetGameState() {
        SetTimeScaleOfGame(1f);
        _player.ResetControls();
        _player.ResetPhysics();
        _player.ResetSlowedTime();
        if (_slowDownTimeCoroutine != null) {
            StopCoroutine(_slowDownTimeCoroutine);
        }
        if (BGM.Instance) {
            BGM.Instance.SetSlowMusic();
        }
    }

    public void PlayerDied() {
        StartCoroutine(LoadTransitionAfter(_deathDelay));
    }

    IEnumerator LoadTransitionAfter(float seconds) {
        yield return new WaitForSecondsRealtime(seconds);
        ResetGameState();
        _transition.LoadScene();
    }

    public void SlowDownTime() {
        _slowDownTimeCoroutine = StartCoroutine(RelayEffectSlowDownTime());
    }

    private void SetTimeScaleOfGame(float scale) {
        Time.timeScale = scale;
        _player.SetOpacityForSlowedEffect(scale);
        if (BGM.Instance) {
            BGM.Instance.SetAudioSpeed(scale);
        }
    }

    // This effect is caused by the SlowDownTime relay. It gradually slows the game down until 0, then player dies.
    public IEnumerator RelayEffectSlowDownTime() {
        for (float f = 1.0f; f > 0f; f -= 0.1f) {
            yield return new WaitForSecondsRealtime(1.2f);
            SetTimeScaleOfGame(f);
        }
        SetTimeScaleOfGame(0f);
        yield return new WaitForSecondsRealtime(1.5f);
        PlayerDied();
    }
}
