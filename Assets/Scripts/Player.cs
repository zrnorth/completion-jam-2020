﻿using System.Collections;
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
    private float _horizAccelerationAirborne = 2f;
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
    [SerializeField]
    private LayerMask _groundMask;

    // State vars
    private int _numDoubleJumpsRemaining;
    private float _nextJumpTime;
    private float _lastGroundedTime = GROUNDED;
    private bool _invertedControls = false;

    // Component references
    private Rigidbody2D _rb;
    private Collider2D _collider;
    private Animator _anim;
    private GameManager _gameManager;
    private float _originalGravityScale;

    public void SetGroundMask(LayerMask layerMask) {
        _groundMask = layerMask;
    }

    private void Start() {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _anim = GetComponent<Animator>();
        if (!_collider) {
            _collider = GetComponent<CircleCollider2D>();
        }
        _numDoubleJumpsRemaining = _doubleJumps;
        _nextJumpTime = Time.time + _jumpCooldown;
        _originalGravityScale = _rb.gravityScale;
        _gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    private void GetInputAndCalculateMoment() {
        bool isGrounded = _lastGroundedTime == GROUNDED || Time.time < _lastGroundedTime + _coyoteTime;

        float newXVelocity = _rb.velocity.x;
        float horizInput = GetHorizontalInput();
        float horiz = isGrounded ? horizInput * _horizAcceleration : horizInput * _horizAccelerationAirborne;
        newXVelocity = Mathf.Clamp(newXVelocity + horiz, -_maxHorizSpeed, _maxHorizSpeed);

        _rb.velocity = new Vector2(newXVelocity, _rb.velocity.y);


        bool canJump = isGrounded || _numDoubleJumpsRemaining > 0;

        if (JumpInputThisFrame() && Time.time > _nextJumpTime && canJump) {
            Jump();
            if (!isGrounded) {
                _numDoubleJumpsRemaining--;
            }
        }
        // Reduce gravity while jump is held, so that the player can more
        // granularly choose how high to jump.
        if (JumpInputBeingHeldDown() && _rb.velocity.y > 0f) {

            _rb.gravityScale = _originalGravityScale / _jumpingGravityReduction;
        } else {
            _rb.gravityScale = _originalGravityScale;
        }
    }

    private void Jump() {
        _rb.velocity = new Vector2(_rb.velocity.x, _jumpForce);
        _nextJumpTime = Time.time + _jumpCooldown;
    }

    private void Update() {
        GetInputAndCalculateMoment();
        _anim.SetFloat("Horizontal Speed", _rb.velocity.x);
    }

    private void FixedUpdate() {
        UpdateGrounded();
    }

    public void FreezePlayer() {
        _rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        switch (other.gameObject.tag) {
            case "KillsPlayer":
                _gameManager.PlayerDied();
                return;
            case "Enemy":
                HitEnemy(other.gameObject.GetComponent<Enemy>());
                return;
            case "Goal":
                other.GetComponent<SceneTransitioner>().LoadScene();
                return;
            default: return;
        }
    }

    private void HitEnemy(Enemy enemy) {
        // If the enemy isn't set to dynamic movement, we've frozen it with a Relay ability, so don't trigger this.
        if (enemy.GetComponent<Rigidbody2D>().bodyType != RigidbodyType2D.Dynamic) {
            return;
        }

        // Check if we stomped the enemy. 
        float verticalHeightAboveEnemy = transform.position.y - enemy.transform.position.y;
        if (verticalHeightAboveEnemy > _collider.bounds.extents.y) {
            Jump();
            _anim.SetTrigger("Stomped");
            enemy.Stomp();
        } // If we didn't stomp the enemy, check if we're allowed to touch this enemy. If not, we die.
        else if (!enemy.GetPlayerCanTouch()) {
            _gameManager.PlayerDied();
            return;
        }
    }

    private void UpdateGrounded() {
        float distance = _collider.bounds.extents.y + 0.1f;

        RaycastHit2D hit = Physics2D.BoxCast(_collider.bounds.center, _collider.bounds.size, 0f, Vector2.down, distance, _groundMask);
        if (hit.collider != null) {
            _lastGroundedTime = GROUNDED;
            _anim.SetBool("Grounded", true);
            _numDoubleJumpsRemaining = _doubleJumps;
        } else {
            if (_lastGroundedTime == GROUNDED) { // if we just jumped, set the current time to the last grounded time
                _lastGroundedTime = Time.time;
                _anim.SetBool("Grounded", false);
            }
        }
    }

    private float GetHorizontalInput() {
        if (_invertedControls) {
            return Input.GetAxis("Vertical");
        }
        return Input.GetAxisRaw("Horizontal");
    }

    private bool JumpInputThisFrame() {
        if (_invertedControls) {
            return Input.GetKeyDown(KeyCode.D);
        }
        return Input.GetKeyDown(KeyCode.Space);
    }

    private bool JumpInputBeingHeldDown() {
        if (_invertedControls) {
            return Input.GetKey(KeyCode.D);
        }
        return Input.GetKey(KeyCode.Space);
    }

    public void InvertControls() {
        _invertedControls = true;
    }
    public void ResetControls() {
        _invertedControls = false;
    }
}
