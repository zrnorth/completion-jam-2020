using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class will be extended for different Relay effects. This is the default -- just enables the goal.
public class Relay : MonoBehaviour
{
    [SerializeField]
    private GameObject _goal;
    [SerializeField]
    private Grid _levelGrid;

    public void RelayLevel() {
        _goal.SetActive(true);
    }
}
