using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private float _speed;
    [SerializeField]
    private LayerMask _groundMask;

    private Rigidbody2D _rb;
    private BoxCollider2D _collider;
    bool _grounded = false;
    float _currSpeed;

    private void Start() {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _currSpeed = 0f;
    }

    private void Update() {
        UpdateGroundedAndMaybeChangeDirection();
        Vector2 vel = _rb.velocity;
        vel.x = _currSpeed;
        _rb.velocity = vel;
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

    public void Stomp() {
        // We got killed
        // Todo: play some anim here
        Destroy(gameObject);
    }
}
