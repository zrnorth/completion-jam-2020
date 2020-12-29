using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReskinAnimation : MonoBehaviour
{
    [SerializeField]
    private string _spriteSheetName;

    private void LateUpdate() {
        var subSprites = Resources.LoadAll<Sprite>("Enemy Variations/" + _spriteSheetName);
        foreach (var renderer in GetComponentsInChildren<SpriteRenderer>()) {
            string spriteName = renderer.sprite.name;
            var newSprite = Array.Find(subSprites, item => item.name == spriteName);
            if (newSprite != null) {
                renderer.sprite = newSprite;
            }

        }
    }

}
