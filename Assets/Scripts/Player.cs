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
    private float _speed = 90f;
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
    private bool _didJumpThisFrame;
    private Vector2 _moment = new Vector2(0, 0);
    private float _lastGroundedTime = GROUNDED;
    private bool _isOnOneWayPlatform = false;

    // Component references
    private Rigidbody2D _rb;
    private Collider2D _collider;
    private LayerMask _groundLayerMask;
    private float _originalGravityScale;

    private void ApplyMoment() {
        // Apply jump forces. Double-jumps should have the same height, so reset
        // the y velocity before adding the moment.
        if (_didJumpThisFrame) {
            _rb.velocity = new Vector2(_rb.velocity.x, 0);
            _didJumpThisFrame = false;
        }
        _rb.AddForce(_moment);
        _moment = new Vector2(0, 0);
    }

    private void GetInputAndCalculateMoment() {
        float horiz = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        _moment.x += horiz * _speed;
        bool isPressingDown = vertical <= -1f;

        bool isGrounded = _lastGroundedTime == GROUNDED || Time.time < _lastGroundedTime + _coyoteTime;
        bool canJump = isGrounded || _numDoubleJumpsRemaining > 0;
        if (Input.GetKeyDown(KeyCode.Space)) {
            // Down+Jump = drop through platforms by disabling collision
            if (isGrounded && isPressingDown && _isOnOneWayPlatform) {
                _collider.isTrigger = true;
            }
            // Otherwise it's a regular jump
            else if (Time.time > _nextJumpTime && canJump) {
                _moment.y += _jumpForce;
                _nextJumpTime = Time.time + _jumpCooldown;
                _didJumpThisFrame = true;
                if (!isGrounded) {
                    _numDoubleJumpsRemaining--;
                }
            }
        }

        // Reduce gravity while jump is held, so that the player can more
        // granularly choose how high to jump.
        if (Input.GetKey(KeyCode.Space) && this._rb.velocity.y > 0f) {
            _rb.gravityScale = _originalGravityScale / _jumpingGravityReduction;
        } else {
            _rb.gravityScale = _originalGravityScale;
        }
    }

    private void Start() {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        if (!_collider) {
            _collider = GetComponent<CircleCollider2D>();
        }
        _groundLayerMask = LayerMask.GetMask("Platform");
        _numDoubleJumpsRemaining = _doubleJumps;
        _nextJumpTime = Time.time + _jumpCooldown;
        _originalGravityScale = _rb.gravityScale;
    }

    private void Update() {
        GetInputAndCalculateMoment();
    }

    private void FixedUpdate() {
        ApplyMoment();
        UpdateGrounded();
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.transform.tag == "Platform" && _rb.velocity.y <= 0) {
            _numDoubleJumpsRemaining = _doubleJumps;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.transform.tag == "Platform") {
            _collider.isTrigger = false;
        }
    }

    public void FreezePlayer() {
        _rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private void UpdateGrounded() {
        float distance = _collider.bounds.extents.y + 0.1f;
        // TODO: do two raycasts at each x bound instead of one at the center.
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector3.down, distance, _groundLayerMask);

        if (_lastGroundedTime == GROUNDED && hit.collider == null) {
            _lastGroundedTime = Time.time;
            _isOnOneWayPlatform = false;
        } else if (_lastGroundedTime != GROUNDED && hit.collider != null) {
            _lastGroundedTime = GROUNDED;
            PlatformEffector2D effector = hit.collider.GetComponent<PlatformEffector2D>();
            if (effector) {
                _isOnOneWayPlatform = effector.useOneWay;
            }
        }
    }
}
