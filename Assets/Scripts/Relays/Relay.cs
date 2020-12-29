using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class will be extended for different Relay effects. This is the default -- just enables the goal.
public class Relay : MonoBehaviour
{
    enum Effect
    {
        None,
        InvertMap,
        RotateMap180,
        TurnCivsIntoPlatforms,
        SlowDownTime,
        InvertControls,
        Bouncy,
        CrazyCivs
    }

    [SerializeField]
    private Effect _effect;
    [SerializeField]
    private GameObject _goal;
    [SerializeField]
    private GameObject _enemiesContainer;
    [SerializeField]
    private GameManager _gameManager;
    [SerializeField]
    private Player _player;
    [SerializeField]
    private Grid _levelGrid;

    public void RelayLevel() {
        _goal.SetActive(true);
        switch (_effect) {
            case Effect.InvertMap:
                InvertMap();
                return;
            case Effect.RotateMap180:
                RotateMap180();
                return;
            case Effect.TurnCivsIntoPlatforms:
                TurnCivsIntoPlatforms();
                return;
            case Effect.SlowDownTime:
                _gameManager.SlowDownTime();
                return;
            case Effect.InvertControls:
                _player.InvertControls();
                return;
            case Effect.Bouncy:
                _player.SetBouncy();
                return;
            case Effect.CrazyCivs:
                EnableCrazyCivs();
                return;
            case Effect.None:
            default:
                return;
        }
    }

    // Relay effect. Swaps out the various tilemaps with their "inverted" variants.
    // Wish there was a cleaner way to do this, but for now we just swap out the tilemaps.
    private void InvertMap() {
        GameObject groundTilemap = _levelGrid.transform.Find("Ground Tilemap").gameObject;
        GameObject deathTilemap = _levelGrid.transform.Find("Death Tilemap").gameObject;
        GameObject invertedGroundTilemap = _levelGrid.transform.Find("Inverted Ground Tilemap").gameObject;
        GameObject invertedDeathTilemap = _levelGrid.transform.Find("Inverted Death Tilemap").gameObject;

        groundTilemap.SetActive(false);
        deathTilemap.SetActive(false);
        invertedGroundTilemap.SetActive(true);
        invertedDeathTilemap.SetActive(true);
    }

    // Relay effect. Inverts the gravity in the scene.
    private void RotateMap180() {
        _levelGrid.transform.eulerAngles = new Vector3(0, 0, 180);
    }

    private void TurnCivsIntoPlatforms() {
        // First, delete the platform part of the map
        _levelGrid.transform.Find("Ground Tilemap").gameObject.SetActive(false);
        // Next, iterate every enemy and freeze them in place
        foreach (Transform childTransform in _enemiesContainer.transform) {
            childTransform.GetComponent<Enemy>().Freeze();
        }
        // Finally, update the player's Grounded function to look for enemies instead of ground tiles
        _player.SetGroundMask(LayerMask.GetMask("Enemies"));
    }

    private void EnableCrazyCivs() {
        foreach (Transform childTransform in _enemiesContainer.transform) {
            childTransform.GetComponent<Enemy>().Enrage();
        }
    }
}
