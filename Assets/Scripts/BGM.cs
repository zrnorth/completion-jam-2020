using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGM : MonoBehaviour
{
    private static BGM instance = null;
    public static BGM Instance {
        get { return instance; }
    }

    [SerializeField]
    private AudioClip _slowMusic, _fastMusic;

    private AudioSource[] _audioSources;

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        } else {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        _audioSources = GetComponents<AudioSource>();
    }

    public void SetSlowMusic() {
        foreach (AudioSource source in _audioSources) {
            source.mute = (source.clip != _slowMusic);
        }
    }

    public void SetFastMusic() {
        foreach (AudioSource source in _audioSources) {
            source.mute = (source.clip != _fastMusic);
        }
    }
}
