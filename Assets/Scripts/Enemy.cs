using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float _speed;
    [SerializeField]
    private LayerMask _groundMask;
    [SerializeField]
    private bool _playerCanTouch;
    [SerializeField]
    private Sprite _frozenSprite;

    private Rigidbody2D _rb;
    private BoxCollider2D _collider;
    private Animator _anim;
    private SpriteRenderer _renderer;
    bool _grounded = false;
    float _currSpeed;

    private void Start() {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _anim = GetComponent<Animator>();
        _renderer = GetComponent<SpriteRenderer>();
        _currSpeed = 0f;
    }

    public bool GetPlayerCanTouch() {
        return _playerCanTouch;
    }

    private void Update() {
        // An Relay ability freezes these in place by setting this type.
        if (_rb.bodyType == RigidbodyType2D.Static) return;

        UpdateGroundedAndMaybeChangeDirection();
        Vector2 vel = _rb.velocity;
        vel.x = _currSpeed;
        _rb.velocity = vel;
        _anim.SetFloat("Horizontal Speed", vel.x);
    }
    private void UpdateGroundedAndMaybeChangeDirection() {
        // Check a small bit in front of us (or under our feet if we aren't moving)
        float offsetHoriz = _collider.bounds.extents.x + Mathf.Abs(_currSpeed / 10f);
        if (_currSpeed < 0) {
            offsetHoriz *= -1f;
        }
        float distanceVert = _collider.bounds.extents.y + 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(offsetHoriz, 0), Vector3.down, distanceVert, _groundMask);
        // We are walking off the edge, so change directions
        if (hit.collider == null && _grounded) {
            _currSpeed *= -1;
        }
        // We just landed 
        else if (hit.collider != null && !_grounded) {
            _grounded = true;
            _currSpeed = _speed;
        }
    }

    public bool PlayerCanStompThis() {
        return (GetComponent<Relay>() != null);
    }

    public void Stomp() {
        // We got killed
        // if we are a relay enemy, apply the effect and continue the game
        Relay relay = GetComponent<Relay>();
        if (relay != null) {
            relay.RelayLevel();
        }
        _collider.enabled = false;
        _rb.constraints = RigidbodyConstraints2D.None; // So we can rotate pathetically as we fall off screen
        Destroy(gameObject, 1.5f);
        return;
    }

    public void Enrage() {
        _currSpeed *= 2;
        _speed *= 2;
        _anim.speed *= 2;
    }

    public void Freeze() {
        _rb.bodyType = RigidbodyType2D.Static;
        _anim.enabled = false;
        _renderer.sprite = _frozenSprite;
    }

}
