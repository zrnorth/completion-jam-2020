using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Constants
    // Used in _lastGroundedTime
    private static float GROUNDED = -1;

    // Game scalars
    [SerializeField]
    private float _horizAcceleration = 5f;
    [SerializeField]
    private float _maxHorizSpeed = 90f;
    [SerializeField]
    private float _jumpForce = 2000f;
    [SerializeField]
    private float _jumpingGravityReduction = 4f;
    [SerializeField]
    private int _doubleJumps = 1;
    [SerializeField]
    private float _jumpCooldown = 0.1f;
    [SerializeField]
    private float _coyoteTime = 0.1f;

    // State vars
    private int _numDoubleJumpsRemaining;
    private float _nextJumpTime;
    private float _lastGroundedTime = GROUNDED;

    // Component references
    private Rigidbody2D _rb;
    private Collider2D _collider;
    private float _originalGravityScale;

    private void GetInputAndCalculateMoment() {
        Vector2 newVelocity = _rb.velocity;
        float horiz = Input.GetAxisRaw("Horizontal");
        newVelocity.x = Mathf.Clamp(newVelocity.x + horiz * _horizAcceleration, -_maxHorizSpeed, _maxHorizSpeed);

        bool isGrounded = _lastGroundedTime == GROUNDED || Time.time < _lastGroundedTime + _coyoteTime;
        bool canJump = isGrounded || _numDoubleJumpsRemaining > 0;

        if (Input.GetKeyDown(KeyCode.Space) && Time.time > _nextJumpTime && canJump) {
            newVelocity.y = Mathf.Min(newVelocity.y + _jumpForce, _jumpForce * 1f);
            _nextJumpTime = Time.time + _jumpCooldown;
            if (!isGrounded) {
                _numDoubleJumpsRemaining--;
            }
        }
        // Reduce gravity while jump is held, so that the player can more
        // granularly choose how high to jump.
        if (Input.GetKey(KeyCode.Space) && this._rb.velocity.y > 0f) {
            _rb.gravityScale = _originalGravityScale / _jumpingGravityReduction;
        } else {
            _rb.gravityScale = _originalGravityScale;
        }
        _rb.velocity = newVelocity;
    }

    private void Start() {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        if (!_collider) {
            _collider = GetComponent<CircleCollider2D>();
        }
        _numDoubleJumpsRemaining = _doubleJumps;
        _nextJumpTime = Time.time + _jumpCooldown;
        _originalGravityScale = _rb.gravityScale;
    }

    private void Update() {
        GetInputAndCalculateMoment();
    }

    private void FixedUpdate() {
        UpdateGrounded();
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.transform.tag == "Platform" && _rb.velocity.y <= 0) {
            _numDoubleJumpsRemaining = _doubleJumps;
        }
    }

    public void FreezePlayer() {
        _rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private void UpdateGrounded() {
        float distance = _collider.bounds.extents.y + 0.1f;
        // TODO: do two raycasts at each x bound instead of one at the center
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector3.down, distance);

        if (_lastGroundedTime == GROUNDED && hit.collider == null) {
            _lastGroundedTime = Time.time;
        } else if (_lastGroundedTime != GROUNDED && hit.collider != null) {
            _lastGroundedTime = GROUNDED;
        }
    }
}
