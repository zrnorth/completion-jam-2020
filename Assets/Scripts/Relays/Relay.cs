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
        InvertGravity,
    }

    [SerializeField]
    private Effect _effect;

    [SerializeField]
    private GameObject _goal;
    [SerializeField]
    private Grid _levelGrid;

    public void RelayLevel() {
        _goal.SetActive(true);
        switch (_effect) {
            case Effect.InvertMap:
                InvertMap();
                return;
            case Effect.InvertGravity:
                InvertGravity();
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
    private void InvertGravity() {
        _levelGrid.transform.eulerAngles = new Vector3(0, 0, 180);
    }
}
